import { Component, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LocationService } from '../../core/services/api/location.service';
import { SearchService } from '../../core/services/api/search.service';
import { BookingStateService } from '../../core/services/booking-state.service';
import { RouteService } from '../../core/services/api/route.service';
import { CompanyService } from '../../core/services/api/company.service';
import { LocationModel } from '../../core/models/master.model';
import { TripSearchResult } from '../../core/models/business.model';
import { HomeSliderComponent } from './components/home-slider/home-slider.component';
import { ScheduleService } from '../../core/services/api/schedule.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, HomeSliderComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  locations: LocationModel[] = [];
  sameLocationError = false;
  formSubmitted = false;

  searchParams = {
    tripType: 'one-way',
    fromLocationId: '',
    toLocationId: '',
    transportType: 'Bus',
    departureDate: '',
    returnDate: ''
  };

  today = new Date().toISOString().split('T')[0];

  currentTab: 'outbound' | 'return' = 'outbound';
  
  // Search Results State
  trips: TripSearchResult[] = [];
  filteredTrips: TripSearchResult[] = [];
  isLoading = false;
  hasSearched = false;

  // Filters State
  filterTypes: string[] = [];
  selectedTypes: string[] = [];
  maxPrice: number = 5000;
  currentMaxPrice: number = 5000;

  // Custom dropdown state
  fromDropdownOpen = false;
  toDropdownOpen = false;
  fromSearch = '';
  toSearch = '';

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

  featuredRoutes: any[] = [];
  companies: any[] = [];
  upcomingSchedules: TripSearchResult[] = [];

  upcomingTripsBanners: any[] = [];
  popularRoutesBanners: any[] = [];

  constructor(
    private locationService: LocationService,
    private searchService: SearchService,
    public bookingState: BookingStateService,
    private routeService: RouteService,
    private companyService: CompanyService,
    private scheduleService: ScheduleService,
    private router: Router,
    private elRef: ElementRef,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const todayStr = new Date().toISOString().split('T')[0];
    this.searchParams.departureDate = todayStr;

    this.loadLocations();
    this.loadCompanies();
    this.loadUpcomingSchedules();
    this.loadBanners();
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
        // Load featured routes after locations are ready so getLocationName() works
        this.loadFeaturedRoutes();
      }
    });
  }

  loadBanners() {
    this.http.get<any>(`${environment.apiUrl}/banners`).subscribe({
      next: (res) => {
        if (res.success) {
          this.upcomingTripsBanners = res.data.filter((b: any) => b.position === 'UpcomingTrips');
          this.popularRoutesBanners = res.data.filter((b: any) => b.position === 'PopularRoutes');
        }
      }
    });
  }

  loadFeaturedRoutes() {
    this.routeService.getAll().subscribe((res: any) => {
      if (res.success) {
        const activeRoutes = res.data.filter((r: any) => r.isActive);
        this.featuredRoutes = activeRoutes.slice(0, 8).map((r: any) => {
          return {
            fromLocationId: r.fromLocationID || r.fromLocationId,
            toLocationId: r.toLocationID || r.toLocationId,
            from: this.getLocationName(r.fromLocationID || r.fromLocationId),
            to: this.getLocationName(r.toLocationID || r.toLocationId),
            distance: r.distanceKM || r.distance,
            estimatedHours: r.estimatedHours,
            type: 'Bus', // Defaulting to Bus for now
            image: r.imageUrl || 'assets/images/default-route.jpg'
          };
        });
      }
    });
  }

  loadCompanies() {
    this.companyService.getAll().subscribe((res: any) => {
      if (res.success) {
        this.companies = res.data.filter((c: any) => c.isActive);
      }
    });
  }

  loadUpcomingSchedules() {
    this.scheduleService.getUpcoming().subscribe((res: any) => {
      if (res.success) {
        this.upcomingSchedules = res.data;
      }
    });
  }

  // Custom dropdown handlers
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

  onSearch() {
    this.formSubmitted = true;
    this.sameLocationError = false;

    if (!this.searchParams.fromLocationId || !this.searchParams.toLocationId || !this.searchParams.departureDate) {
      return;
    }

    if (this.searchParams.tripType === 'round-way' && !this.searchParams.returnDate) {
      return;
    }

    if (this.searchParams.fromLocationId === this.searchParams.toLocationId) {
      this.sameLocationError = true;
      return;
    }

    this.bookingState.setTripType(this.searchParams.tripType as any);
    this.currentTab = 'outbound';
    this.performSearch();
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
        travelDate: date
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
    this.performSearch();
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
    this.performSearch();
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
  }

  resetFilters() {
    this.currentMaxPrice = this.maxPrice;
    this.selectedTypes = this.filterTypes.slice();
    this.applyFilters();
  }

  viewSeats(scheduleId: number) {
    const trip = this.trips.find(t => t.scheduleId === scheduleId);
    if (trip) {
      if (this.currentTab === 'outbound') {
        this.bookingState.setOutboundTrip(trip);
      } else {
        this.bookingState.setReturnTrip(trip);
      }
    }
    this.router.navigate(['/schedules', scheduleId]);
  }

  swapLocations() {
    const temp = this.searchParams.fromLocationId;
    this.searchParams.fromLocationId = this.searchParams.toLocationId;
    this.searchParams.toLocationId = temp;
    this.sameLocationError = false;
  }

  quickSearch(route: any) {
    const fromLoc = this.locations.find(l => {
      const name = l.locationName;
      return name.toLowerCase().includes(route.from.toLowerCase());
    });
    const toLoc = this.locations.find(l => {
      const name = l.locationName;
      return name.toLowerCase().includes(route.to.toLowerCase());
    });
    if (fromLoc) {
      const id = fromLoc.locationId;
      this.searchParams.fromLocationId = id.toString();
    }
    if (toLoc) {
      const id = toLoc.locationId;
      this.searchParams.toLocationId = id.toString();
    }
    this.searchParams.transportType = route.type;
    this.searchParams.departureDate = this.today;
    
    // Instead of routing, we just set the params and call onSearch()
    if (fromLoc && toLoc) {
      this.onSearch();
      window.scrollTo({ top: 350, behavior: 'smooth' });
    }
  }

  getLocationName(id: string | number): string {
    if (!id || !this.locations || this.locations.length === 0) return 'Loading...';
    const loc = this.locations.find(l => l.locationId && l.locationId.toString() === id.toString());
    return loc ? loc.locationName : 'Unknown';
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
