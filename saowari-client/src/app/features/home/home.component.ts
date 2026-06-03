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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { ScrollAnimateDirective } from '../../shared/directives/scroll-animate.directive';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, HomeSliderComponent, MatDatepickerModule, MatNativeDateModule, MatInputModule, ScrollAnimateDirective],
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

  get totalRoutesCount(): number {
    return this.featuredRoutes.length; 
  }

  get totalOperatorsCount(): number {
    return this.companies.length; 
  }

  get totalLocationsCount(): number {
    return this.locations.length; 
  }

  upcomingTripsBanners: any[] = [];
  popularRoutesBanners: any[] = [];
  topHeroBanners: any[] = [];
  middleContentBanners: any[] = [];
  transportPartnersBanners: any[] = [];

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
          this.topHeroBanners = res.data.filter((b: any) => b.position === 'TopHero');
          this.middleContentBanners = res.data.filter((b: any) => b.position === 'MiddleContent');
          this.transportPartnersBanners = res.data.filter((b: any) => b.position === 'TransportPartners' || b.position === 'BottomFooter' || b.position === 'LeftSidebar' || b.position === 'RightSidebar');
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
    this.router.navigate(['/search'], { queryParams: this.searchParams });
  }


  swapLocations() {
    const temp = this.searchParams.fromLocationId;
    this.searchParams.fromLocationId = this.searchParams.toLocationId;
    this.searchParams.toLocationId = temp;
    this.sameLocationError = false;
  }

  onDateChange(field: 'departureDate' | 'returnDate', date: Date) {
    if (date) {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      this.searchParams[field] = `${year}-${month}-${day}`;
    } else {
      this.searchParams[field] = '';
    }
  }

  viewSeats(scheduleId: number) {
    const trip = this.upcomingSchedules.find(t => t.scheduleId === scheduleId);
    if (trip) {
      this.bookingState.setTripType('one-way');
      this.bookingState.setOutboundTrip(trip);
    }
    this.router.navigate(['/schedules', scheduleId]);
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

  getVerticalBanners(banners: any[], side: 'left' | 'right'): any[] {
    const vertical = banners.filter(b => b.sizeTemplate === 'Vertical' || b.sizeTemplate === 'Square');
    if (side === 'left') {
      return vertical.filter((_, i) => i % 2 === 0); // Evens to the left
    } else {
      return vertical.filter((_, i) => i % 2 !== 0); // Odds to the right
    }
  }

  getHorizontalBanners(banners: any[]): any[] {
    return banners.filter(b => b.sizeTemplate !== 'Vertical' && b.sizeTemplate !== 'Square');
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

  getBannerClass(sizeTemplate: string): string {
    // Returns Tailwind classes for width and aspect ratio to ensure banners look normal, not gigantic.
    switch (sizeTemplate) {
      case 'Square': 
        return 'w-full max-w-[220px] aspect-square'; 
      case 'Vertical': 
        return 'w-full max-w-[200px] aspect-[9/16]'; 
      case 'Standard': 
        return 'w-full max-w-[480px] aspect-video'; 
      case 'Horizontal':
      default:
        return 'w-full max-w-[900px] h-[120px] md:h-[180px] lg:h-[220px]'; 
    }
  }
}
