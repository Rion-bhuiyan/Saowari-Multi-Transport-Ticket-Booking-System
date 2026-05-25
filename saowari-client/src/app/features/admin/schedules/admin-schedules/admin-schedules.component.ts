import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { RouteService } from '../../../../core/services/api/route.service';
import { VehicleService } from '../../../../core/services/api/vehicle.service';
import { ScheduleStatusService } from '../../../../core/services/api/schedule-status.service';
import { UserService } from '../../../../core/services/api/user.service';
import { LocationService } from '../../../../core/services/api/location.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { SeatClassService } from '../../../../core/services/api/seat-class.service';
import { SeatPricingService } from '../../../../core/services/api/seat-pricing.service';

@Component({
  selector: 'app-admin-schedules',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-schedules.component.html',
  styleUrls: ['./admin-schedules.component.css']
})
export class AdminSchedulesComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';

  routesList: any[] = [];
  vehiclesList: any[] = [];
  scheduleStatuses: any[] = [];
  driversList: any[] = [];
  supervisorsList: any[] = [];
  locationsList: any[] = [];
  seatClasses: any[] = [];

  // Modal state
  isModalOpen = false;
  editingItem: any = null;
  model: any = this.defaultModel();

  constructor(
    private svc: ScheduleService,
    private routeService: RouteService,
    private vehicleService: VehicleService,
    private scheduleStatusService: ScheduleStatusService,
    private userService: UserService,
    private locationService: LocationService,
    private notification: NotificationService,
    public authService: AuthService,
    private router: Router,
    private seatClassService: SeatClassService,
    private seatPricingService: SeatPricingService
  ) {}

  defaultModel() {
    return {
      routeId: null,
      vehicleId: null,
      driverInformtionId: null,
      supervisorId: null,
      departureTime: '',
      arrivalTime: '',
      ticketPrice: 0,
      availableSeats: 40,
      scheduleStatusId: 1,
      departureLocations: [] as any[],
      seatClassPricings: [] as any[]
    };
  }

  ngOnInit(): void {
    this.load();
    this.routeService.getAll().subscribe((res: any) => { if (res.success) this.routesList = res.data || []; });
    this.vehicleService.getAll().subscribe((res: any) => { if (res.success) this.vehiclesList = res.data || []; });
    this.scheduleStatusService.getAll().subscribe((res: any) => { if (res.success) this.scheduleStatuses = res.data || []; });
    this.locationService.getAll().subscribe((res: any) => { if (res.success) this.locationsList = res.data || []; });
    this.seatClassService.getAll().subscribe((res: any) => { 
      if (res.success) {
        this.seatClasses = (res.data || []).map((item: any) => ({
          seatClassId: item.seatClassId || item.SeatClassId || item.seatClassID || item.SeatClassID || item.id || 0,
          seatClassName: item.seatClassName || item.SeatClassName || ''
        }));
      }
    });
    // Load drivers and supervisors filtered by role from the users endpoint
    this.userService.getAll().subscribe((res: any) => {
      if (res.success) {
        const users = res.data || [];
        this.driversList = users.filter((u: any) =>
          (u.roleName || u.RoleName || '').toLowerCase() === 'driver' &&
          (u.driverInformtionId || u.DriverInformtionId)
        );
        this.supervisorsList = users.filter((u: any) =>
          (u.roleName || u.RoleName || '').toLowerCase() === 'supervisor' &&
          (u.supervisorId || u.SupervisorId)
        );
      }
    });
  }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { 
          this.items = (res.data || []).map((item: any) => {
            if (item.seatClassPricings) {
              item.seatClassPricings = item.seatClassPricings.map((scp: any) => ({
                seatClassId: scp.seatClassId || scp.SeatClassId || scp.seatClassID || scp.SeatClassID || 0,
                seatClassName: scp.seatClassName || scp.SeatClassName || '',
                price: scp.price || scp.Price || 0
              }));
            }
            return item;
          });
          this.applyFilter(); 
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    this.filtered = q ? this.items.filter(i =>
      JSON.stringify(i).toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: any) {
    if (item) {
      this.editingItem = item;
      const depRaw = item.departureDateTime || item.departureTime;
      const arrRaw = item.arrivalDateTime || item.arrivalTime;
      this.model = {
        routeId: item.routeID || item.routeId,
        vehicleId: item.vehicleID || item.vehicleId,
        driverInformtionId: item.driverInformtionID || item.driverInformtionId,
        supervisorId: item.supervisorID || item.supervisorId || null,
        departureTime: depRaw ? depRaw.substring(0, 16) : '',
        arrivalTime: arrRaw ? arrRaw.substring(0, 16) : '',
        ticketPrice: item.basePrice || item.ticketPrice || 0,
        availableSeats: item.availableSeats || 40,
        scheduleStatusId: item.scheduleStatusID || item.scheduleStatusId || 1,
        departureLocations: item.departureLocations ? item.departureLocations.filter((dl: any, index: number, self: any[]) =>
          index === self.findIndex((t) => (t.locationID || t.locationId) === (dl.locationID || dl.locationId))
        ).map((dl: any) => ({
          locationId: dl.locationID || dl.locationId,
          time: dl.time ? (typeof dl.time === 'string' ? dl.time.substring(0, 5) : dl.time) : '06:00',
          searchQuery: dl.locationName || this.getLocationName(dl.locationID || dl.locationId),
          showDropdown: false,
          filteredLocations: [...this.locationsList]
        })) : [],
        seatClassPricings: item.seatClassPricings ? item.seatClassPricings.map((scp: any) => ({
          seatClassId: scp.seatClassId || scp.SeatClassId || scp.seatClassID || scp.SeatClassID || 0,
          seatClassName: scp.seatClassName || this.getSeatClassName(scp.seatClassId || scp.SeatClassId || scp.seatClassID || scp.SeatClassID),
          price: scp.price
        })) : []
      };
    } else {
      this.editingItem = null;
      this.model = this.defaultModel();
    }
    this.isModalOpen = true;
  }

  onVehicleChange(vehicleId: any) {
    const vId = Number(vehicleId);
    if (!vId) {
      this.model.seatClassPricings = [];
      return;
    }
    const vehicle = this.vehiclesList.find(v => (v.vehicleID || v.vehicleId || v.id) === vId);
    if (vehicle && vehicle.seats) {
      const uniqueClassIds = [...new Set(vehicle.seats.map((s: any) => s.seatClassId || s.SeatClassId || s.seatClassID || s.SeatClassID))] as number[];
      this.model.seatClassPricings = uniqueClassIds.map(classId => ({
        seatClassId: classId,
        seatClassName: this.getSeatClassName(classId),
        price: this.model.ticketPrice || 0
      }));
    } else {
      this.model.seatClassPricings = [];
    }
  }

  getSeatClassName(id: number): string {
    const sc = this.seatClasses.find(c => c.seatClassId === id);
    return sc ? sc.seatClassName : `Class ${id}`;
  }

  addSeatClassPricing() {
    this.model.seatClassPricings.push({
      seatClassId: null,
      seatClassName: '',
      price: 0
    });
  }

  removeSeatClassPricing(idx: number) {
    this.model.seatClassPricings.splice(idx, 1);
  }

  closeModal() {
    this.isModalOpen = false;
  }

  save() {
    if (!this.model.routeId || !this.model.vehicleId || !this.model.driverInformtionId ||
        !this.model.departureTime || !this.model.arrivalTime) {
      this.notification.error('Please fill all required fields including Driver', 'Validation');
      return;
    }
    if (new Date(this.model.arrivalTime) <= new Date(this.model.departureTime)) {
      this.notification.error('Arrival time must be after departure time', 'Validation');
      return;
    }

    const payload: any = {
      routeId: Number(this.model.routeId),
      vehicleId: Number(this.model.vehicleId),
      driverInformtionId: Number(this.model.driverInformtionId),
      departureDateTime: this.model.departureTime.length === 16 ? this.model.departureTime + ':00' : this.model.departureTime,
      arrivalDateTime: this.model.arrivalTime.length === 16 ? this.model.arrivalTime + ':00' : this.model.arrivalTime,
      basePrice: this.model.seatClassPricings && this.model.seatClassPricings.length > 0
        ? Math.min(...this.model.seatClassPricings.map((p: any) => Number(p.price) || 0))
        : (Number(this.model.ticketPrice) || 0),
      scheduleStatusId: Number(this.model.scheduleStatusId) || 1,
      seatClassPricings: this.model.seatClassPricings.map((p: any) => ({
        seatClassId: Number(p.seatClassId),
        price: Number(p.price)
      }))
    };
    if (this.model.supervisorId) {
      payload.supervisorId = Number(this.model.supervisorId);
    }

    // Attach departure locations
    if (this.model.departureLocations && this.model.departureLocations.length > 0) {
      payload.departureLocations = this.model.departureLocations
        .filter((dl: any) => dl.locationId && dl.time)
        .map((dl: any) => ({
          locationID: Number(dl.locationId),
          time: dl.time.length === 5 ? `${dl.time}:00` : dl.time
        }));
    } else {
      payload.departureLocations = [];
    }

    const request = this.editingItem
      ? this.svc.update(this.editingItem.scheduleID || this.editingItem.scheduleId || this.editingItem.id, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Schedule saved successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save');
        }
      },
      error: (err: any) => {
        const msg = err?.error?.message || 'An error occurred. Please check your inputs.';
        this.notification.error(msg);
      }
    });
  }

  deleteItem(id: number) {
    if (!id) { this.notification.error('Cannot delete: invalid ID'); return; }
    if (confirm('Are you sure you want to delete this schedule?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Schedule deleted');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete');
          }
        },
        error: () => this.notification.error('An error occurred while deleting')
      });
    }
  }

  viewSeatMap(id: number) {
    this.router.navigate(['/admin/schedules', id, 'seat-map']);
  }

  // ── boarding points ────────────────────────────────────────

  addDepartureLocation() {
    this.model.departureLocations.push({
      locationId: null,
      time: '06:00',
      searchQuery: '',
      showDropdown: false,
      filteredLocations: [...this.locationsList]
    });
  }

  removeDepartureLocation(index: number) {
    this.model.departureLocations.splice(index, 1);
  }

  filterLocations(dl: any) {
    const q = (dl.searchQuery || '').toLowerCase();
    dl.filteredLocations = q
      ? this.locationsList.filter(l => l.locationName.toLowerCase().includes(q))
      : [...this.locationsList];
  }

  selectLocation(dl: any, loc: any) {
    dl.locationId = loc.locationID || loc.locationId || loc.id;
    dl.searchQuery = loc.locationName;
    dl.showDropdown = false;
  }

  hideDropdown(dl: any) {
    setTimeout(() => {
      dl.showDropdown = false;
      if (!dl.locationId) {
        dl.searchQuery = '';
      } else {
        dl.searchQuery = this.getLocationName(dl.locationId);
      }
    }, 200);
  }

  // ── helpers ────────────────────────────────────────────────

  getLocationName(id: number): string {
    if (!id) return '?';
    const loc = this.locationsList.find(l => (l.locationID || l.locationId || l.id) === id);
    return loc ? loc.locationName : `Loc ${id}`;
  }

  getRouteLabel(route: any): string {
    const fromId = route.fromLocationID || route.fromLocationId;
    const toId   = route.toLocationID   || route.toLocationId;
    if (fromId && toId && this.locationsList.length) {
      return `${this.getLocationName(fromId)} → ${this.getLocationName(toId)}`;
    }
    return route.routeName || `Route ${route.routeID || route.routeId || route.id}`;
  }

  getRouteName(id: number): string {
    if (!id) return '';
    const route = this.routesList.find(r => (r.routeID || r.routeId || r.id) === id);
    return route ? this.getRouteLabel(route) : `Route ${id}`;
  }

  getVehicleName(id: number): string {
    if (!id) return '';
    const v = this.vehiclesList.find(v => (v.vehicleID || v.vehicleId || v.id) === id);
    return v ? (v.vehicleName || `Vehicle ${id}`) : `Vehicle ${id}`;
  }

  getStatusName(id: number): string {
    if (!id) return '';
    const s = this.scheduleStatuses.find(s => (s.scheduleStatusId || s.scheduleStatusID || s.id) === id);
    return s ? (s.scheduleStatusName || s.statusName || `Status ${id}`) : `Status ${id}`;
  }

  getStatusBadge(id: number): string {
    const name = this.getStatusName(id).toLowerCase();
    if (name.includes('scheduled') || name.includes('active')) return 'badge-success';
    if (name.includes('cancel')) return 'badge-error';
    if (name.includes('complet')) return 'badge-info';
    return 'badge-warning';
  }
}
