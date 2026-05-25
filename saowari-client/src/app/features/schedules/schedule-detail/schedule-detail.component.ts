import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ScheduleService } from '../../../core/services/api/schedule.service';
import { SearchService } from '../../../core/services/api/search.service';
import { LocationService } from '../../../core/services/api/location.service';
import { ScheduleModel, LocationModel } from '../../../core/models/master.model';
import { SeatMapItem } from '../../../core/models/business.model';
import { NotificationService } from '../../../core/services/notification.service';
import { BookingStateService } from '../../../core/services/booking-state.service';

declare var google: any;

@Component({
  selector: 'app-schedule-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './schedule-detail.component.html',
  styleUrls: ['./schedule-detail.component.css']
})
export class ScheduleDetailComponent implements OnInit, OnDestroy {
  scheduleId!: number;
  schedule!: ScheduleModel;
  seatMap: SeatMapItem[] = [];
  
  // Seat selection
  selectedSeats: SeatMapItem[] = [];
  maxSeatsPerUser = 4;
  
  isLoading = true;

  // Deck structure (for bus layout, simplified)
  seatRows: { deckName: string, rows: SeatMapItem[][] }[] = [];

  // Timer State
  countdownSeconds: number = 0;
  timerInterval: any = null;
  readonly TIMEOUT_SECONDS = 180; // 3 minutes

  // Seat Layout Config
  config: any = { Rows: 10, Columns: 4, AisleAfterColumn: 2, IsDoubleDecker: false, ContinuousBackRow: false };

  locations: LocationModel[] = [];
  selectedBoardingPoint?: string;
  gMapInstance?: any;
  markers: any[] = [];
  gmapsLoaded = false;

  private premiumMapStyles = [
    { elementType: "geometry", stylers: [{ color: "#0F172A" }] },
    { elementType: "labels.text.fill", stylers: [{ color: "#94A3B8" }] },
    { elementType: "labels.text.stroke", stylers: [{ color: "#0F172A" }] },
    { featureType: "administrative.country", elementType: "geometry.stroke", stylers: [{ color: "#334155" }] },
    { featureType: "administrative.province", elementType: "geometry.stroke", stylers: [{ color: "#334155" }] },
    { featureType: "landscape.natural", elementType: "geometry", stylers: [{ color: "#1E293B" }] },
    { featureType: "poi", elementType: "geometry", stylers: [{ color: "#1E293B" }] },
    { featureType: "poi", elementType: "labels.text.fill", stylers: [{ color: "#64748B" }] },
    { featureType: "road", elementType: "geometry", stylers: [{ color: "#334155" }] },
    { featureType: "road.labels.text.fill", stylers: [{ color: "#94A3B8" }] },
    { featureType: "road.highway", elementType: "geometry", stylers: [{ color: "#1E3A8A" }] },
    { featureType: "road.highway", elementType: "geometry.stroke", stylers: [{ color: "#0F172A" }] },
    { featureType: "water", elementType: "geometry", stylers: [{ color: "#0B1528" }] },
    { featureType: "water", elementType: "labels.text.fill", stylers: [{ color: "#475569" }] }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private scheduleService: ScheduleService,
    private searchService: SearchService,
    private locationService: LocationService,
    private notification: NotificationService,
    public bookingState: BookingStateService
  ) {}

  ngOnInit(): void {
    this.loadLocations();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.scheduleId = +id;
        this.loadScheduleDetails();
      }
    });
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  startTimer() {
    if (this.timerInterval) return;
    this.countdownSeconds = this.TIMEOUT_SECONDS;
    this.timerInterval = setInterval(() => {
      this.countdownSeconds--;
      if (this.countdownSeconds <= 0) {
        this.clearTimer();
        this.selectedSeats = [];
        this.notification.error('Session expired. Seats have been released.');
      }
    }, 1000);
  }

  clearTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    this.countdownSeconds = 0;
  }

  formatCountdown(): string {
    const mins = Math.floor(this.countdownSeconds / 60);
    const secs = this.countdownSeconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  loadScheduleDetails() {
    this.isLoading = true;
    this.scheduleService.getById(this.scheduleId).subscribe({
      next: (res) => {
        if (res.success) {
          this.schedule = res.data;
          if (this.schedule.seatLayoutConfig) {
            try {
              this.config = JSON.parse(this.schedule.seatLayoutConfig);
            } catch (e) {
              console.error("Error parsing seatLayoutConfig", e);
            }
          }
          this.loadSeatMap();
          this.loadGoogleMapsApi().then(() => {
            setTimeout(() => {
              this.initGoogleMap();
            }, 150);
          }).catch(err => {
            console.error('Error loading Google Maps:', err);
          });
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadSeatMap() {
    this.searchService.getSeatMap(this.scheduleId).subscribe((res: any) => {
      if (res.success) {
        const seatMap: SeatMapItem[] = res.data;
        
        const decksMap = new Map<string, Map<string, SeatMapItem[]>>();

        const sortedSeats = [...seatMap].sort((a, b) => {
          return a.seatNumber.localeCompare(b.seatNumber, undefined, { numeric: true, sensitivity: 'base' });
        });

        sortedSeats.forEach(seat => {
          let deck = 'Main Deck';
          let rowPrefix = seat.seatNumber;

          if (this.config.IsDoubleDecker && (seat.seatNumber.startsWith('L') || seat.seatNumber.startsWith('U'))) {
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

        this.seatRows = decksArray;
      }
    });
  }

  toggleSeat(seat: SeatMapItem) {
    if (seat.isBooked || seat.seatStatusName !== 'Available') return;

    const index = this.selectedSeats.findIndex(s => s.seatId === seat.seatId);
    if (index > -1) {
      this.selectedSeats.splice(index, 1);
      if (this.selectedSeats.length === 0) {
        this.clearTimer();
      }
    } else {
      if (this.selectedSeats.length >= this.maxSeatsPerUser) {
        this.notification.warning(`You can select a maximum of ${this.maxSeatsPerUser} seats.`);
        return;
      }
      this.selectedSeats.push(seat);
      if (this.selectedSeats.length === 1) {
        this.startTimer();
      }
    }
  }

  isSelected(seatId: number): boolean {
    return this.selectedSeats.some(s => s.seatId === seatId);
  }

  getTotalPrice(): number {
    return this.selectedSeats.reduce((sum, seat) => sum + (seat.price ?? seat.Price ?? this.schedule?.basePrice ?? 0), 0);
  }

  proceedToBook() {
    if (this.selectedSeats.length === 0) {
      this.notification.warning('Please select at least one seat to proceed.');
      return;
    }

    const seatIds = this.selectedSeats.map(s => s.seatId);
    const seatNumbers = this.selectedSeats.map(s => s.seatNumber);
    const state = this.bookingState.currentState;

    if (state.tripType === 'round-way') {
      if (state.outbound.schedule?.scheduleId === this.scheduleId) {
        this.bookingState.setOutboundSeats(seatIds, seatNumbers);
        if (this.selectedBoardingPoint) {
          this.bookingState.setOutboundBoardingPoint(this.selectedBoardingPoint);
        }
        this.notification.success('Outbound seats selected! Now select your return trip.');
        this.router.navigate(['/search'], { queryParams: { tab: 'return' }, queryParamsHandling: 'merge' });
        return;
      } else if (state.return.schedule?.scheduleId === this.scheduleId) {
        this.bookingState.setReturnSeats(seatIds, seatNumbers);
        if (this.selectedBoardingPoint) {
          this.bookingState.setReturnBoardingPoint(this.selectedBoardingPoint);
        }
        this.router.navigate(['/booking']);
        return;
      }
    }

    // Default One-Way fallback
    this.bookingState.setTripType('one-way');
    this.bookingState.setOutboundSeats(seatIds, seatNumbers);
    if (this.selectedBoardingPoint) {
      this.bookingState.setOutboundBoardingPoint(this.selectedBoardingPoint);
    }
    
    this.router.navigate(['/booking'], { 
      queryParams: { 
        scheduleId: this.scheduleId,
        seats: seatIds.join(',') 
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

  initGoogleMap() {
    if (!this.schedule) return;

    const mapElement = document.getElementById('gmap-detail');
    if (!mapElement) {
      console.warn('Map container element #gmap-detail not found');
      return;
    }

    const startName = this.getRouteStart() || 'Start City';
    const endName = this.getRouteEnd() || 'Destination City';

    const matchedStart = this.locations.find(l => l.locationName.toLowerCase() === startName.toLowerCase());
    const matchedEnd = this.locations.find(l => l.locationName.toLowerCase() === endName.toLowerCase());

    const startCoords = this.getCoordinates(startName, matchedStart?.latitude, matchedStart?.longitude);
    const endCoords = this.getCoordinates(endName, matchedEnd?.latitude, matchedEnd?.longitude);

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
    this.gMapInstance = map;
    this.markers = [];

    const bounds = new google.maps.LatLngBounds();

    // 1. Draw Start Marker
    const startMarker = new google.maps.Marker({
      position: startCoords,
      map: map,
      title: `Origin: ${startName}`,
      icon: {
        path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
        fillColor: '#3B82F6',
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
        fillColor: '#EF4444',
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
    const boardingPoints = this.schedule.departureLocations || [];
    const routeCoords: any[] = [startCoords];

    boardingPoints.forEach((bp: any) => {
      const bpName = bp.locationName || 'Boarding Point';
      const bpCoords = this.getCoordinates(bpName, bp.latitude, bp.longitude);
      routeCoords.push(bpCoords);
      bounds.extend(bpCoords);

      const isSelectedPoint = this.selectedBoardingPoint === bpName;
      const bpMarker = new google.maps.Marker({
        position: bpCoords,
        map: map,
        title: bpName,
        icon: {
          path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
          fillColor: isSelectedPoint ? '#F59E0B' : '#10B981',
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
        google.maps.event.addListenerOnce(bpInfo, 'domready', () => {
          const btn = document.getElementById(`bp-btn-${bp.locationID}`);
          if (btn) {
            btn.onclick = () => {
              this.selectBoardingPoint(bpName);
              bpInfo.close();
            };
          }
        });
      });

      this.markers.push({ name: bpName, marker: bpMarker, data: bp });
    });

    routeCoords.push(endCoords);

    // 4. Draw Polyline
    const routeLine = new google.maps.Polyline({
      path: routeCoords,
      geodesic: true,
      strokeColor: '#3B82F6',
      strokeOpacity: 0.8,
      strokeWeight: 4,
      map: map
    });

    map.fitBounds(bounds);
    const listener = google.maps.event.addListener(map, "idle", () => {
      if (map.getZoom()! > 14) map.setZoom(14);
      google.maps.event.removeListener(listener);
    });
  }

  updateBoardingMarkers() {
    if (!this.markers) return;
    this.markers.forEach((item: any) => {
      const isSelected = this.selectedBoardingPoint === item.name;
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

  selectBoardingPoint(pointName: string) {
    this.selectedBoardingPoint = pointName;
    const state = this.bookingState.currentState;
    if (state.tripType === 'round-way' && state.return.schedule?.scheduleId === this.scheduleId) {
      this.bookingState.setReturnBoardingPoint(pointName);
    } else {
      this.bookingState.setOutboundBoardingPoint(pointName);
    }
    this.notification.success(`Boarding Point Selected: ${pointName}`);
    this.updateBoardingMarkers();
  }

  Math = Math;

  formatTime(dateString: string): string {
    if (!dateString) return '';
    return new Date(dateString).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  getRouteStart(): string {
    return this.schedule?.route?.split('-')[0] || '';
  }

  getRouteEnd(): string {
    const parts = this.schedule?.route?.split('-');
    if (parts && parts.length > 1) {
      return parts[1];
    }
    return this.schedule?.route || '';
  }
}
