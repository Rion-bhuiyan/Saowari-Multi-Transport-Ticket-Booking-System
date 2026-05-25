import { Component, OnInit, OnDestroy, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../../core/services/api/search.service';
import { LocationService } from '../../core/services/api/location.service';
import { ScheduleService } from '../../core/services/api/schedule.service';
import { TripSearchResult, SeatMapItem } from '../../core/models/business.model';
import { LocationModel } from '../../core/models/master.model';
import { BookingStateService } from '../../core/services/booking-state.service';
import { NotificationService } from '../../core/services/notification.service';

declare var google: any;

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit, OnDestroy {
  searchParams = {
    tripType: 'one-way',
    fromLocationId: '',
    toLocationId: '',
    departureDate: '',
    returnDate: '',
    transportType: 'Bus'
  };

  currentTab: 'outbound' | 'return' = 'outbound';

  locations: LocationModel[] = [];
  trips: TripSearchResult[] = [];
  filteredTrips: TripSearchResult[] = [];
  isLoading = false;
  hasSearched = false;

  // Filters & Sorting
  filterTypes: string[] = [];
  selectedTypes: string[] = [];
  maxPrice: number = 5000;
  currentMaxPrice: number = 5000;
  sortBy: string = 'departureAsc'; // default sort

  // Custom Dropdown State
  fromDropdownOpen = false;
  toDropdownOpen = false;
  fromSearch = '';
  toSearch = '';
  sameLocationError = false;

  // Inline Expansion State
  expandedTripId: number | null = null;
  seatMaps: { [scheduleId: number]: { rows: any[], selected: SeatMapItem[], isLoading: boolean, config: any, countdownSeconds: number, timerInterval: any, selectedBoardingPoint?: string, scheduleDetail?: any, gMapInstance?: any, markers?: any[] } } = {};
  maxSeatsPerUser = 4;
  readonly TIMEOUT_SECONDS = 180; // 3 minutes

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private searchService: SearchService,
    private locationService: LocationService,
    private scheduleService: ScheduleService,
    public bookingState: BookingStateService,
    private notification: NotificationService,
    private elRef: ElementRef
  ) {}

  ngOnDestroy(): void {
    // Clear all running timers on component destroy
    Object.values(this.seatMaps).forEach(m => {
      if (m.timerInterval) clearInterval(m.timerInterval);
    });
  }

  ngOnInit(): void {
    this.loadLocations();
    
    this.route.queryParams.subscribe(params => {
      if (params['tripType']) this.searchParams.tripType = params['tripType'];
      if (params['fromLocationId']) this.searchParams.fromLocationId = params['fromLocationId'];
      if (params['toLocationId']) this.searchParams.toLocationId = params['toLocationId'];
      if (params['departureDate']) this.searchParams.departureDate = params['departureDate'];
      if (params['returnDate']) this.searchParams.returnDate = params['returnDate'];
      if (params['transportType']) this.searchParams.transportType = params['transportType'];

      if (params['tab']) this.currentTab = params['tab'] as 'outbound' | 'return';
      else this.currentTab = 'outbound';

      this.bookingState.setTripType(this.searchParams.tripType as any);

      if (this.searchParams.fromLocationId && this.searchParams.toLocationId) {
        this.performSearch();
      }
    });
  }

  loadLocations() {
    this.locationService.getAll().subscribe(res => {
      if (res.success) {
        this.locations = res.data
          .filter((l: any) => l.isActive)
          .map((l: any) => ({
            ...l,
            locationId: l.locationID || l.locationId || l.id,
            locationName: l.locationName || l.name || 'Unknown'
          }));
      }
    });
  }

  // --- Custom Dropdown Logic ---
  get filteredFromLocations() {
    const q = this.fromSearch.toLowerCase();
    return this.locations.filter(l => {
      const id = l.locationId;
      const name = l.locationName;
      return l && id != null && name &&
        name.toLowerCase().includes(q) &&
        id.toString() !== this.searchParams.toLocationId;
    });
  }

  get filteredToLocations() {
    const q = this.toSearch.toLowerCase();
    return this.locations.filter(l => {
      const id = l.locationId;
      const name = l.locationName;
      return l && id != null && name &&
        name.toLowerCase().includes(q) &&
        id.toString() !== this.searchParams.fromLocationId;
    });
  }

  get selectedFromName(): string {
    if (!this.locations?.length || !this.searchParams.fromLocationId) return '';
    const loc = this.locations.find(l => {
      const id = l.locationId;
      return l && id != null && id.toString() === this.searchParams.fromLocationId;
    });
    return loc ? loc.locationName : '';
  }

  get selectedToName(): string {
    if (!this.locations?.length || !this.searchParams.toLocationId) return '';
    const loc = this.locations.find(l => {
      const id = l.locationId;
      return l && id != null && id.toString() === this.searchParams.toLocationId;
    });
    return loc ? loc.locationName : '';
  }

  openFromDropdown() {
    this.fromDropdownOpen = true;
    this.toDropdownOpen = false;
    this.fromSearch = '';
    setTimeout(() => (document.getElementById('from-search-input') as HTMLInputElement)?.focus(), 50);
  }

  selectFrom(loc: LocationModel) {
    const id = loc.locationId;
    this.searchParams.fromLocationId = id.toString();
    this.fromDropdownOpen = false;
    this.fromSearch = '';
    this.sameLocationError = false;
  }

  openToDropdown() {
    this.toDropdownOpen = true;
    this.fromDropdownOpen = false;
    this.toSearch = '';
    setTimeout(() => (document.getElementById('to-search-input') as HTMLInputElement)?.focus(), 50);
  }

  selectTo(loc: LocationModel) {
    const id = loc.locationId;
    this.searchParams.toLocationId = id.toString();
    this.toDropdownOpen = false;
    this.toSearch = '';
    this.sameLocationError = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.fromDropdownOpen = false;
      this.toDropdownOpen = false;
    }
  }

  onSearchSubmit() {
    // Update URL which will trigger the subscription
    this.router.navigate(['/search'], { queryParams: this.searchParams });
  }

  performSearch() {
    this.isLoading = true;
    this.hasSearched = true;
    this.trips = [];
    this.filteredTrips = [];
    
    let fromId = this.searchParams.fromLocationId;
    let toId = this.searchParams.toLocationId;
    let date = this.searchParams.departureDate;

    if (this.currentTab === 'return') {
      fromId = this.searchParams.toLocationId;
      toId = this.searchParams.fromLocationId;
      date = this.searchParams.returnDate;
    }

    if (!date) {
        this.isLoading = false;
        return;
    }

    const apiParams = {
        transportType: this.searchParams.transportType,
        fromLocationId: fromId,
        toLocationId: toId,
        travelDate: date,
        passengers: 1
    };

    this.searchService.searchTrips(apiParams).subscribe({
      next: (res) => {
        if (res.success) {
          this.trips = res.data;
          this.extractFilters();
          this.applyFilters();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  switchTab(tab: 'outbound' | 'return') {
    this.currentTab = tab;
    this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { tab: tab },
        queryParamsHandling: 'merge'
    });
  }

  get dateRange() {
    const activeDateStr = this.currentTab === 'return' ? this.searchParams.returnDate : this.searchParams.departureDate;
    if (!activeDateStr) return [];

    const activeDate = new Date(activeDateStr);
    const dates = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Center around activeDate: 3 days before, active date, 3 days after
    // But don't go before today
    let startDate = new Date(activeDate);
    startDate.setDate(startDate.getDate() - 3);

    if (startDate < today) {
      startDate = new Date(today);
    }

    for (let i = 0; i < 7; i++) {
      const d = new Date(startDate);
      d.setDate(startDate.getDate() + i);
      
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      const dateStr = `${year}-${month}-${day}`;

      dates.push({
        dateString: dateStr,
        label: d.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' }),
        isActive: dateStr === activeDateStr
      });
    }
    return dates;
  }

  selectQuickDate(dateStr: string) {
    if (this.currentTab === 'return') {
      this.searchParams.returnDate = dateStr;
    } else {
      this.searchParams.departureDate = dateStr;
    }
    this.onSearchSubmit();
  }

  swapLocations() {
    const temp = this.searchParams.fromLocationId;
    this.searchParams.fromLocationId = this.searchParams.toLocationId;
    this.searchParams.toLocationId = temp;
  }

  extractFilters() {
    const types = new Set<string>();
    let maxP = 0;
    this.trips.forEach(t => {
      if (t.vehicleType) types.add(t.vehicleType);
      if (t.basePrice > maxP) maxP = t.basePrice;
    });
    this.filterTypes = Array.from(types);
    this.maxPrice = maxP > 0 ? maxP : 5000;
    this.currentMaxPrice = this.maxPrice;
    this.selectedTypes = [...this.filterTypes];
  }

  toggleTypeFilter(type: string) {
    const idx = this.selectedTypes.indexOf(type);
    if (idx > -1) {
      this.selectedTypes.splice(idx, 1);
    } else {
      this.selectedTypes.push(type);
    }
    this.applyFilters();
  }

  onPriceChange() {
    this.applyFilters();
  }

  applyFilters() {
    this.filteredTrips = this.trips.filter(t => {
      const typeMatch = t.vehicleType ? this.selectedTypes.includes(t.vehicleType) : true;
      const priceMatch = t.basePrice <= this.currentMaxPrice;
      return typeMatch && priceMatch;
    });
    this.sortTrips();
  }

  onSortChange() {
    this.sortTrips();
  }

  sortTrips() {
    this.filteredTrips.sort((a, b) => {
      switch (this.sortBy) {
        case 'priceAsc':
          return a.basePrice - b.basePrice;
        case 'priceDesc':
          return b.basePrice - a.basePrice;
        case 'departureAsc':
          return new Date(a.departureDateTime).getTime() - new Date(b.departureDateTime).getTime();
        case 'departureDesc':
          return new Date(b.departureDateTime).getTime() - new Date(a.departureDateTime).getTime();
        case 'boardingTimeAsc':
          const timeA = new Date(a.boardingTime || a.departureDateTime).getTime();
          const timeB = new Date(b.boardingTime || b.departureDateTime).getTime();
          return timeA - timeB;
        case 'seatsDesc':
          return b.availableSeats - a.availableSeats;
        default:
          return new Date(a.departureDateTime).getTime() - new Date(b.departureDateTime).getTime();
      }
    });
  }

  resetFilters() {
    this.currentMaxPrice = this.maxPrice;
    this.selectedTypes = this.filterTypes.slice();
    this.sortBy = 'departureAsc';
    this.applyFilters();
  }

  setTripType(type: 'one-way' | 'round-way') {
    this.searchParams.tripType = type;
    if (type === 'one-way') {
      this.searchParams.returnDate = '';
    }
  }

  toggleExpandTrip(scheduleId: number) {
    if (this.expandedTripId === scheduleId) {
      // Collapsing — clear this trip's timer
      if (this.seatMaps[scheduleId]?.timerInterval) {
        clearInterval(this.seatMaps[scheduleId].timerInterval);
        this.seatMaps[scheduleId].timerInterval = null;
        this.seatMaps[scheduleId].countdownSeconds = 0;
      }
      this.expandedTripId = null;
    } else {
      this.expandedTripId = scheduleId;
      if (!this.seatMaps[scheduleId]) {
        this.loadSeatMap(scheduleId);
      } else {
        // If seatMap is already loaded but we expanded it again, re-trigger map init
        const trip = this.trips.find(t => t.scheduleId === scheduleId);
        if (trip) {
          this.fetchScheduleDetailAndInitMap(scheduleId, trip);
        }
      }
    }
  }

  loadSeatMap(scheduleId: number) {
    const trip = this.trips.find(t => t.scheduleId === scheduleId);
    let layoutConfig = { Rows: 10, Columns: 4, AisleAfterColumn: 2, IsDoubleDecker: false, ContinuousBackRow: false };
    if (trip && trip.seatLayoutConfig) {
      try { layoutConfig = JSON.parse(trip.seatLayoutConfig); } catch (e) {}
    }

    this.seatMaps[scheduleId] = {
      rows: [],
      selected: [],
      isLoading: true,
      config: layoutConfig,
      countdownSeconds: 0,
      timerInterval: null,
      selectedBoardingPoint: '',
      markers: [],
      gMapInstance: null
    };

    this.searchService.getSeatMap(scheduleId).subscribe({
      next: (res) => {
        if (res.success) {
          const seatMap: SeatMapItem[] = res.data;
          
          const decksMap = new Map<string, Map<string, SeatMapItem[]>>();

          const sortedSeats = [...seatMap].sort((a, b) => {
            return a.seatNumber.localeCompare(b.seatNumber, undefined, { numeric: true, sensitivity: 'base' });
          });

          sortedSeats.forEach(seat => {
            let deck = 'Main Deck';
            let rowPrefix = seat.seatNumber;

            if (layoutConfig.IsDoubleDecker && (seat.seatNumber.startsWith('L') || seat.seatNumber.startsWith('U'))) {
              deck = seat.seatNumber.startsWith('L') ? 'Lower Deck' : 'Upper Deck';
              rowPrefix = seat.seatNumber.substring(1);
            }

            let rowMatch = rowPrefix.match(/^[a-zA-Z]+/);
            let row = rowMatch ? rowMatch[0] : 'Other';

            if (!decksMap.has(deck)) decksMap.set(deck, new Map());
            if (!decksMap.get(deck)!.has(row)) decksMap.get(deck)!.set(row, []);

            decksMap.get(deck)!.get(row)!.push(seat);
          });

          // Convert to Array of Decks containing Array of Rows
          const decksArray = Array.from(decksMap.entries()).map(([deckName, rowsMap]) => ({
            deckName,
            rows: Array.from(rowsMap.values())
          }));

          this.seatMaps[scheduleId].rows = decksArray;
        }
        this.seatMaps[scheduleId].isLoading = false;
        
        // Fetch full schedule details for maps and departure locations
        this.fetchScheduleDetailAndInitMap(scheduleId, trip);
      },
      error: () => {
        this.seatMaps[scheduleId].isLoading = false;
      }
    });
  }

  fetchScheduleDetailAndInitMap(scheduleId: number, trip: any) {
    this.scheduleService.getById(scheduleId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.seatMaps[scheduleId].scheduleDetail = res.data;
          
          // Now safely load google maps API and init the map!
          this.loadGoogleMapsApi().then(() => {
            // Wait slightly for DOM to render the map div container
            setTimeout(() => {
              this.initGoogleMapForSchedule(scheduleId, trip);
            }, 150);
          }).catch(err => {
            console.error('Error loading Google Maps:', err);
          });
        }
      },
      error: (err) => {
        console.error('Failed to load schedule detail', err);
      }
    });
  }

  private premiumMapStyles = [
    { elementType: "geometry", stylers: [{ color: "#0F172A" }] }, // Slate 900
    { elementType: "labels.text.fill", stylers: [{ color: "#94A3B8" }] }, // Slate 400
    { elementType: "labels.text.stroke", stylers: [{ color: "#0F172A" }] },
    { featureType: "administrative.country", elementType: "geometry.stroke", stylers: [{ color: "#334155" }] },
    { featureType: "administrative.province", elementType: "geometry.stroke", stylers: [{ color: "#334155" }] },
    { featureType: "landscape.natural", elementType: "geometry", stylers: [{ color: "#1E293B" }] }, // Slate 800
    { featureType: "poi", elementType: "geometry", stylers: [{ color: "#1E293B" }] },
    { featureType: "poi", elementType: "labels.text.fill", stylers: [{ color: "#64748B" }] },
    { featureType: "road", elementType: "geometry", stylers: [{ color: "#334155" }] }, // Slate 700
    { featureType: "road", elementType: "labels.text.fill", stylers: [{ color: "#94A3B8" }] },
    { featureType: "road.highway", elementType: "geometry", stylers: [{ color: "#1E3A8A" }] }, // Indigo 900
    { featureType: "road.highway", elementType: "geometry.stroke", stylers: [{ color: "#0F172A" }] },
    { featureType: "water", elementType: "geometry", stylers: [{ color: "#0B1528" }] }, // Dark Blue
    { featureType: "water", elementType: "labels.text.fill", stylers: [{ color: "#475569" }] }
  ];

  private gmapsLoaded = false;
  private loadGoogleMapsApi(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.gmapsLoaded || (window as any).google?.maps) {
        resolve();
        return;
      }
      
      const existingScript = document.getElementById('google-maps-script');
      if (existingScript) {
        const interval = setInterval(() => {
          if ((window as any).google?.maps) {
            clearInterval(interval);
            this.gmapsLoaded = true;
            resolve();
          }
        }, 100);
        return;
      }

      const script = document.createElement('script');
      script.id = 'google-maps-script';
      script.src = `https://maps.googleapis.com/maps/api/js?key=&libraries=geometry`;
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.gmapsLoaded = true;
        resolve();
      };
      script.onerror = (err) => {
        reject(err);
      };
      document.head.appendChild(script);
    });
  }

  private getCoordinates(locName: string, dbLat?: number, dbLng?: string): { lat: number, lng: number } {
    if (dbLat && dbLng) {
      const parsedLat = typeof dbLat === 'number' ? dbLat : parseFloat(dbLat as any);
      const parsedLng = parseFloat(dbLng);
      if (!isNaN(parsedLat) && !isNaN(parsedLng) && parsedLat !== 0 && parsedLng !== 0) {
        return { lat: parsedLat, lng: parsedLng };
      }
    }

    const dict: { [key: string]: { lat: number, lng: number } } = {
      'dhaka': { lat: 23.8103, lng: 90.4125 },
      'chittagong': { lat: 22.3569, lng: 91.7832 },
      'chattogram': { lat: 22.3569, lng: 91.7832 },
      'sylhet': { lat: 24.8949, lng: 91.8687 },
      'cox': { lat: 21.4272, lng: 91.9702 },
      'cox\'s bazar': { lat: 21.4272, lng: 91.9702 },
      'coxsbazar': { lat: 21.4272, lng: 91.9702 },
      'rajshahi': { lat: 24.3636, lng: 88.6241 },
      'khulna': { lat: 22.8456, lng: 89.5403 },
      'barisal': { lat: 22.7010, lng: 90.3535 },
      'barishal': { lat: 22.7010, lng: 90.3535 },
      'rangpur': { lat: 25.7500, lng: 89.2500 },
      'mymensingh': { lat: 24.7471, lng: 90.4203 },
      'bogra': { lat: 24.8481, lng: 89.3730 },
      'bogura': { lat: 24.8481, lng: 89.3730 },
      'jessore': { lat: 23.1634, lng: 89.2182 },
      'jashore': { lat: 23.1634, lng: 89.2182 },
      'comilla': { lat: 23.4607, lng: 91.1809 },
      'cumilla': { lat: 23.4607, lng: 91.1809 },
      'gabtoli': { lat: 23.7776, lng: 90.3340 },
      'arambagh': { lat: 23.7314, lng: 90.4193 },
      'gabtoli counter': { lat: 23.7779, lng: 90.3343 },
      'gabtoli bus terminal': { lat: 23.7785, lng: 90.3333 },
      'sayedabad': { lat: 23.7196, lng: 90.4328 },
      'sayedabad counter': { lat: 23.7192, lng: 90.4331 },
      'mohakhali': { lat: 23.7788, lng: 90.4005 },
      'mohakhali counter': { lat: 23.7792, lng: 90.4008 },
      'kalabagan': { lat: 23.7508, lng: 90.3802 },
      'panthapath': { lat: 23.7514, lng: 90.3905 }
    };

    const cleanName = locName.toLowerCase().trim();
    for (const key of Object.keys(dict)) {
      if (cleanName.includes(key)) {
        return dict[key];
      }
    }
    return { lat: 23.8103, lng: 90.4125 };
  }

  initGoogleMapForSchedule(scheduleId: number, trip: any) {
    const mapState = this.seatMaps[scheduleId];
    if (!mapState || !mapState.scheduleDetail) return;

    const mapElement = document.getElementById(`gmap-${scheduleId}`);
    if (!mapElement) {
      console.warn(`Map container element #gmap-${scheduleId} not found`);
      return;
    }

    const scheduleDetail = mapState.scheduleDetail;
    const startName = trip.fromLocation || 'Start City';
    const endName = trip.toLocation || 'Destination City';

    // Get Start and Destination Coords
    const matchedStart = this.locations.find(l => l.locationName.toLowerCase() === startName.toLowerCase());
    const matchedEnd = this.locations.find(l => l.locationName.toLowerCase() === endName.toLowerCase());

    const startCoords = this.getCoordinates(startName, matchedStart?.latitude, matchedStart?.longitude);
    const endCoords = this.getCoordinates(endName, matchedEnd?.latitude, matchedEnd?.longitude);

    // Initialize Map
    const mapOptions: any = {
      center: startCoords,
      zoom: 8,
      styles: this.premiumMapStyles,
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
      zoomControl: true,
      zoomControlOptions: {
        position: google.maps.ControlPosition.RIGHT_BOTTOM
      }
    };

    const map = new google.maps.Map(mapElement, mapOptions);
    mapState.gMapInstance = map;
    mapState.markers = [];

    const bounds = new google.maps.LatLngBounds();

    // 1. Draw Start Marker
    const startMarker = new google.maps.Marker({
      position: startCoords,
      map: map,
      title: `Origin: ${startName}`,
      icon: {
        path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
        fillColor: '#3B82F6', // Blue-500
        fillOpacity: 1,
        strokeColor: '#FFFFFF',
        strokeWeight: 2,
        scale: 1
      }
    });
    bounds.extend(startCoords);

    const startInfo = new google.maps.InfoWindow({
      content: `<div class="p-2 text-slate-800"><p class="font-bold text-xs uppercase tracking-wider text-blue-600 mb-0.5">Start City</p><p class="font-semibold text-sm">${startName}</p></div>`
    });
    startMarker.addListener('click', () => {
      startInfo.open(map, startMarker);
    });

    // 2. Draw Destination Marker
    const endMarker = new google.maps.Marker({
      position: endCoords,
      map: map,
      title: `Destination: ${endName}`,
      icon: {
        path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
        fillColor: '#EF4444', // Red-500
        fillOpacity: 1,
        strokeColor: '#FFFFFF',
        strokeWeight: 2,
        scale: 1.1
      }
    });
    bounds.extend(endCoords);

    const endInfo = new google.maps.InfoWindow({
      content: `<div class="p-2 text-slate-800"><p class="font-bold text-xs uppercase tracking-wider text-red-600 mb-0.5">Destination</p><p class="font-semibold text-sm">${endName}</p></div>`
    });
    endMarker.addListener('click', () => {
      endInfo.open(map, endMarker);
    });

    // 3. Draw Intermediate Boarding Points
    const boardingPoints = scheduleDetail.departureLocations || [];
    const routeCoords: any[] = [startCoords];

    boardingPoints.forEach((bp: any) => {
      const bpName = bp.locationName || 'Boarding Point';
      const bpCoords = this.getCoordinates(bpName, bp.latitude, bp.longitude);
      routeCoords.push(bpCoords);
      bounds.extend(bpCoords);

      const isSelectedPoint = mapState.selectedBoardingPoint === bpName;
      const bpMarker = new google.maps.Marker({
        position: bpCoords,
        map: map,
        title: bpName,
        icon: {
          path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
          fillColor: isSelectedPoint ? '#F59E0B' : '#10B981', // Amber if selected, Emerald if not
          fillOpacity: 1,
          strokeColor: '#FFFFFF',
          strokeWeight: 2,
          scale: isSelectedPoint ? 1.2 : 0.9
        }
      });

      const timeStr = bp.time ? bp.time.substring(0, 5) : '00:00';
      const infoContent = `
        <div class="p-3 text-slate-800 max-w-[200px]">
          <p class="font-extrabold text-[10px] uppercase tracking-widest text-emerald-600 mb-1">Boarding Point</p>
          <h6 class="font-bold text-sm text-slate-900 mb-1">${bpName}</h6>
          <p class="text-xs text-slate-500 mb-2"><i class="far fa-clock mr-1"></i> Time: <b>${timeStr}</b></p>
          <button id="bp-btn-${bp.locationID}" class="w-full py-1.5 px-3 rounded-lg text-white font-bold text-xs bg-emerald-500 hover:bg-emerald-600 shadow-sm transition-colors text-center cursor-pointer">
            ${isSelectedPoint ? '✓ Selected' : 'Select Stop'}
          </button>
        </div>
      `;

      const bpInfo = new google.maps.InfoWindow({
        content: infoContent
      });

      bpMarker.addListener('click', () => {
        bpInfo.open(map, bpMarker);
        
        // Listen for InfoWindow DOM ready to attach click event to selector button
        google.maps.event.addListenerOnce(bpInfo, 'domready', () => {
          const btn = document.getElementById(`bp-btn-${bp.locationID}`);
          if (btn) {
            btn.onclick = () => {
              this.selectBoardingPoint(scheduleId, bpName);
              bpInfo.close();
            };
          }
        });
      });

      mapState.markers?.push({ name: bpName, marker: bpMarker, data: bp });
    });

    routeCoords.push(endCoords);

    // 4. Draw Polyline connecting the route "outside"
    const routeLine = new google.maps.Polyline({
      path: routeCoords,
      geodesic: true,
      strokeColor: '#3B82F6',
      strokeOpacity: 0.8,
      strokeWeight: 4,
      map: map
    });

    // Fit map to show all coordinates perfectly
    map.fitBounds(bounds);
    
    // Add small padding
    const listener = google.maps.event.addListener(map, "idle", () => {
      if (map.getZoom()! > 14) map.setZoom(14);
      google.maps.event.removeListener(listener);
    });
  }

  updateBoardingMarkers(scheduleId: number) {
    const mapState = this.seatMaps[scheduleId];
    if (!mapState || !mapState.markers) return;

    mapState.markers.forEach((item: any) => {
      const isSelected = mapState.selectedBoardingPoint === item.name;
      item.marker.setIcon({
        path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
        fillColor: isSelected ? '#F59E0B' : '#10B981',
        fillOpacity: 1,
        strokeColor: '#FFFFFF',
        strokeWeight: 2,
        scale: isSelected ? 1.2 : 0.9
      });
    });
  }

  selectBoardingPoint(scheduleId: number, pointName: string) {
    const mapState = this.seatMaps[scheduleId];
    if (mapState) {
      mapState.selectedBoardingPoint = pointName;
      if (this.currentTab === 'outbound') {
        this.bookingState.setOutboundBoardingPoint(pointName);
      } else {
        this.bookingState.setReturnBoardingPoint(pointName);
      }
      this.notification.success(`Boarding Point Selected: ${pointName}`);
      this.updateBoardingMarkers(scheduleId);
    }
  }

  toggleSeat(scheduleId: number, seat: SeatMapItem) {
    if (seat.isBooked || seat.seatStatusName !== 'Available') return;

    const mapState = this.seatMaps[scheduleId];
    const index = mapState.selected.findIndex(s => s.seatId === seat.seatId);
    
    if (index > -1) {
      mapState.selected.splice(index, 1);
      // Stop timer if no seats are selected
      if (mapState.selected.length === 0) {
        clearInterval(mapState.timerInterval);
        mapState.timerInterval = null;
        mapState.countdownSeconds = 0;
      }
    } else {
      if (mapState.selected.length >= this.maxSeatsPerUser) {
        this.notification.warning(`You can select a maximum of ${this.maxSeatsPerUser} seats.`);
        return;
      }
      mapState.selected.push(seat);
      // Start timer when first seat is selected
      if (mapState.selected.length === 1 && !mapState.timerInterval) {
        mapState.countdownSeconds = this.TIMEOUT_SECONDS;
        mapState.timerInterval = setInterval(() => {
          mapState.countdownSeconds--;
          if (mapState.countdownSeconds <= 0) {
            clearInterval(mapState.timerInterval);
            mapState.timerInterval = null;
            mapState.selected = [];
            this.notification.error('Session expired. Your held seats have been released.');
          }
        }, 1000);
      }
    }
  }

  isSelected(scheduleId: number, seatId: number): boolean {
    return this.seatMaps[scheduleId]?.selected.some(s => s.seatId === seatId) || false;
  }

  getTotalPrice(trip: TripSearchResult): number {
    const selectedSeats = this.seatMaps[trip.scheduleId]?.selected || [];
    if (selectedSeats.length === 0) return 0;
    
    return selectedSeats.reduce((total, seat: any) => total + (seat.price ?? seat.Price ?? trip.basePrice), 0);
  }

  proceedToBook(trip: TripSearchResult) {
    const selectedSeats = this.seatMaps[trip.scheduleId]?.selected || [];
    if (selectedSeats.length === 0) {
      this.notification.warning('Please select at least one seat to proceed.');
      return;
    }

    const seatIds = selectedSeats.map(s => s.seatId);
    const seatNumbers = selectedSeats.map(s => s.seatNumber);
    const state = this.bookingState.currentState;

    if (this.currentTab === 'outbound') {
      this.bookingState.setOutboundTrip(trip);
      this.bookingState.setOutboundSeats(seatIds, seatNumbers);
    } else {
      this.bookingState.setReturnTrip(trip);
      this.bookingState.setReturnSeats(seatIds, seatNumbers);
    }

    if (state.tripType === 'round-way' && this.currentTab === 'outbound') {
      this.notification.success('Outbound seats selected! Now select your return trip.');
      this.router.navigate(['/search'], { queryParams: { tab: 'return' }, queryParamsHandling: 'merge' });
      return;
    }

    this.bookingState.setTripType(this.searchParams.tripType as any);
    this.router.navigate(['/booking'], { 
      queryParams: { 
        scheduleId: trip.scheduleId,
        seats: seatIds.join(',') 
      } 
    });
  }

  getLocationName(id: string | number): string {
    if (!id || !this.locations || this.locations.length === 0) return 'Loading...';
    const loc = this.locations.find(l => l.locationId && l.locationId.toString() === id.toString());
    return loc ? loc.locationName : 'Unknown';
  }

  formatSeatTimer(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatTime(dateString: string): string {
    return new Date(dateString).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  formatDuration(start: string, end: string): string {
    const diffMs = new Date(end).getTime() - new Date(start).getTime();
    const diffHrs = Math.floor(diffMs / 3600000);
    const diffMins = Math.round((diffMs % 3600000) / 60000);
    return `${diffHrs}h ${diffMins}m`;
  }
}
