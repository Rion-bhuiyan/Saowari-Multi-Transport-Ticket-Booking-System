import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VehicleService } from '../../../../core/services/api/vehicle.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { VehicleTypeService } from '../../../../core/services/api/vehicle-type.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { SeatClassService, SeatClassModel } from '../../../../core/services/api/seat-class.service';
import { SeatPricingService } from '../../../../core/services/api/seat-pricing.service';

export interface VisualSeat {
  row: number;
  col: number;
  seatNumber: string;
  seatClassId: number;
}
export interface VisualDeck {
  level: number;
  name: string;
  seats: VisualSeat[];
}
export interface VisualLayout {
  mode: string;
  gridWidth: number;
  gridHeight: number;
  isDoubleDecker?: boolean;
  decks: VisualDeck[];
}

@Component({
  selector: 'app-admin-vehicles',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-vehicles.component.html',
  styleUrls: ['./admin-vehicles.component.css']
})
export class AdminVehiclesComponent implements OnInit {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.filtered || this.items || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 15;

  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';

  companies: any[] = [];
  vehicleTypes: any[] = [];
  seatClasses: SeatClassModel[] = [];
  seatClassAssignments: { [seatNumber: string]: number } = {};
  activeAssignmentClassId = 1;

  // Modal state
  isModalOpen = false;
  isLayoutModalOpen = false;
  editingItem: any = null;
  selectedVehicleIdForLayout: number | null = null;
  
  visualLayout: VisualLayout = {
    mode: 'visual',
    gridWidth: 5,
    gridHeight: 12,
    decks: [
      { level: 1, name: 'Main Deck', seats: [] }
    ]
  };
  activeDeckIndex = 0;
  selectedSeat: VisualSeat | null = null;

  model: any = { 
    vehicleName: '', vehicleNumber: '', companyId: null, vehicleTypeId: null, 
    engineNumber: '', engineCC: '', chassisNumber: '',
    capacity: 40, isActive: true,
    isDoubleDecker: false
  };

  constructor(
    private svc: VehicleService,
    private companyService: CompanyService,
    private vehicleTypeService: VehicleTypeService,
    private notification: NotificationService,
    public authService: AuthService,
    private seatClassService: SeatClassService,
    private seatPricingService: SeatPricingService
  ) {}

  ngOnInit(): void { 
    this.load(); 
    this.companyService.getAll().subscribe(res => { if (res.success) this.companies = res.data; });
    this.vehicleTypeService.getAll().subscribe(res => { if (res.success) this.vehicleTypes = res.data; });
    this.seatClassService.getAll().subscribe(res => { 
      if (res.success) {
        this.seatClasses = (res.data || []).map((item: any) => ({
          seatClassId: item.seatClassId || item.SeatClassId || item.seatClassID || item.SeatClassID || item.id || 0,
          seatClassName: item.seatClassName || item.SeatClassName || ''
        }));
        if (this.seatClasses.length > 0) {
          this.activeAssignmentClassId = this.seatClasses[0].seatClassId;
        }
      }
    });
  }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
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
    this.seatClassAssignments = {};
    if (item) {
      this.editingItem = item;
      
      let isDoubleDecker = false;
      if (item.seatLayoutConfig) {
        try {
          const config = JSON.parse(item.seatLayoutConfig);
          isDoubleDecker = config.IsDoubleDecker || config.isDoubleDecker || false;

          const hasDecks = !!(config.decks || config.Decks);
          if (config.mode === 'visual' || config.Mode === 'visual' || hasDecks) {
            this.visualLayout.gridWidth = config.gridWidth || config.GridWidth || 5;
            this.visualLayout.gridHeight = config.gridHeight || config.GridHeight || 12;
            
            const decks = config.decks || config.Decks;
            if (decks && decks.length > 1) {
              isDoubleDecker = true;
            }
            
            this.visualLayout.isDoubleDecker = isDoubleDecker;
            if (decks) {
              this.visualLayout.decks = decks;
              this.visualLayout.decks.forEach((d: any) => {
                if (d.Seats) d.seats = d.Seats;
                if (!d.seats) d.seats = [];
                if (d.Name) d.name = d.Name;
                if (d.Level) d.level = d.Level;
                d.seats.forEach((s: any) => {
                  if (s.Row !== undefined) s.row = s.Row;
                  if (s.Col !== undefined) s.col = s.Col;
                  if (s.SeatNumber) s.seatNumber = s.SeatNumber;
                  if (s.SeatClassId) s.seatClassId = s.SeatClassId;
                });
              });
            } else {
              this.resetVisualLayout();
            }
          } else {
            this.resetVisualLayout();
          }
        } catch(e) {
          this.resetVisualLayout();
        }
      } else {
        this.resetVisualLayout();
      }

      this.model = { 
        ...item,
        capacity: item.totalSeats,
        isDoubleDecker
      };

      if (item.seats && this.visualLayout.decks.length > 0) {
        item.seats.forEach((seat: any) => {
          this.seatClassAssignments[seat.seatNumber] = seat.seatClassId;
        });
        this.visualLayout.decks.forEach(d => {
          d.seats.forEach(s => {
            if (this.seatClassAssignments[s.seatNumber]) {
              s.seatClassId = this.seatClassAssignments[s.seatNumber];
            }
          });
        });
      }

      this.activeAssignmentClassId = this.seatClasses[0]?.seatClassId || 1;
    } else {
      this.editingItem = null;
      this.model = { 
        vehicleName: '', vehicleNumber: '', companyId: null, vehicleTypeId: null, 
        engineNumber: '', engineCC: '', chassisNumber: '',
        capacity: 40, isActive: true,
        isDoubleDecker: false
      };
      this.resetVisualLayout();
      this.activeAssignmentClassId = this.seatClasses[0]?.seatClassId || 1;
    }
    this.activeDeckIndex = 0;
    this.selectedSeat = null;
    this.isModalOpen = true;
  }

  resetVisualLayout() {
    this.visualLayout = {
      mode: 'visual',
      gridWidth: 5,
      gridHeight: 12,
      isDoubleDecker: false,
      decks: [
        { level: 1, name: 'Main Deck', seats: [] }
      ]
    };
  }

  toggleDoubleDecker() {
    if (this.model.isDoubleDecker) {
      if (this.visualLayout.decks.length === 1) {
        this.visualLayout.decks[0].name = 'Lower Deck';
        this.visualLayout.decks.push({ level: 2, name: 'Upper Deck', seats: [] });
      }
    } else {
      if (this.visualLayout.decks.length > 1) {
        this.visualLayout.decks = [this.visualLayout.decks[0]];
        this.visualLayout.decks[0].name = 'Main Deck';
        this.activeDeckIndex = 0;
      }
    }
  }

  closeModal() {
    this.isModalOpen = false;
  }

  updateCapacity() {
    let totalVisualSeats = 0;
    this.visualLayout.decks.forEach(d => totalVisualSeats += d.seats.length);
    if (totalVisualSeats > 0) {
      this.model.capacity = totalVisualSeats;
    }
  }

  getSeatClassNameShort(classId: number): string {
    const sc = this.seatClasses.find(c => c.seatClassId == classId);
    if (!sc) return 'ECO';
    return sc.seatClassName.substring(0, 3).toUpperCase();
  }

  getSeatClassColor(classId: number): string {
    const sc = this.seatClasses.find(c => c.seatClassId == classId);
    if (!sc) return 'bg-saowari-surface border-saowari-border text-saowari-text-secondary';
    const name = sc.seatClassName.toLowerCase();
    if (name.includes('business')) return 'bg-indigo-50 border-indigo-300 text-indigo-700 hover:bg-indigo-100';
    if (name.includes('economy')) return 'bg-slate-50 border-slate-300 text-slate-700 hover:bg-slate-100';
    if (name.includes('first') || name.includes('ac')) return 'bg-emerald-50 border-emerald-300 text-emerald-700 hover:bg-emerald-100';
    if (name.includes('sleeper')) return 'bg-purple-50 border-purple-300 text-purple-700 hover:bg-purple-100';
    return 'bg-amber-50 border-amber-300 text-amber-700 hover:bg-amber-100';
  }

  setActiveAssignmentClass(classId: number) {
    this.activeAssignmentClassId = classId;
    if (this.selectedSeat) {
      this.selectedSeat.seatClassId = classId;
    }
  }

  assignAllSeats(classId: number) {
    if (!classId) return;
    this.visualLayout.decks.forEach(d => {
      d.seats.forEach(s => {
        s.seatClassId = classId;
      });
    });
    this.notification.success('All seats assigned to ' + (this.seatClasses.find(c => c.seatClassId == classId)?.seatClassName || 'selected class'));
  }

  save() {
    // Ensure data consistency before saving
    this.visualLayout.isDoubleDecker = this.model.isDoubleDecker;
    if (!this.model.isDoubleDecker && this.visualLayout.decks.length > 1) {
      this.visualLayout.decks = [this.visualLayout.decks[0]];
      this.visualLayout.decks[0].name = 'Main Deck';
    }

    let totalVisualSeats = 0;
    this.visualLayout.decks.forEach(d => totalVisualSeats += d.seats.length);
    this.model.capacity = totalVisualSeats;

    const payload = {
      companyId: this.model.companyId || 0,
      vehicleName: this.model.vehicleName,
      vehicleNumber: this.model.vehicleNumber,
      engineNumber: this.model.engineNumber,
      engineCC: this.model.engineCC,
      chassisNumber: this.model.chassisNumber,
      vehicleTypeId: this.model.vehicleTypeId,
      totalSeats: this.model.capacity,
      isDoubleDecker: this.model.isDoubleDecker,
      isActive: this.model.isActive,
      visualLayout: this.visualLayout
    };

    const request = this.editingItem 
      ? this.svc.update(this.editingItem.vehicleID || this.editingItem.vehicleId || this.editingItem.id, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success(this.editingItem ? 'Vehicle updated successfully!' : 'Vehicle created successfully!');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save vehicle');
        }
      },
      error: () => this.notification.error('An error occurred while saving vehicle')
    });
  }

  deleteItem(id: number) {
    if (confirm('Are you sure you want to delete this vehicle?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Vehicle deleted');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete');
          }
        },
        error: () => this.notification.error('An error occurred while deleting')
      });
    }
  }

  get activeDeck() {
    return this.visualLayout.decks[this.activeDeckIndex];
  }

  get gridRows() {
    return Array.from({ length: this.visualLayout.gridHeight }, (_, i) => i);
  }

  get gridCols() {
    return Array.from({ length: this.visualLayout.gridWidth }, (_, i) => i);
  }

  getSeatAt(row: number, col: number): VisualSeat | undefined {
    return this.activeDeck.seats.find(s => s.row === row && s.col === col);
  }

  onCellClick(row: number, col: number) {
    const existing = this.getSeatAt(row, col);
    if (existing) {
      this.selectedSeat = existing;
    } else {
      const nextNum = this.activeDeck.seats.length + 1;
      const prefix = this.model.isDoubleDecker ? (this.activeDeck.level === 1 ? 'L' : 'U') : 'S';
      const newSeat: VisualSeat = {
        row,
        col,
        seatNumber: `${prefix}${nextNum}`,
        seatClassId: this.activeAssignmentClassId
      };
      this.activeDeck.seats.push(newSeat);
      this.selectedSeat = newSeat;
      this.updateCapacity();
    }
  }

  removeSelectedSeat() {
    if (this.selectedSeat) {
      this.activeDeck.seats = this.activeDeck.seats.filter(s => s !== this.selectedSeat);
      this.selectedSeat = null;
      this.updateCapacity();
    }
  }

  autoFillLayout(preset: string) {
    this.activeDeck.seats = [];
    const h = this.visualLayout.gridHeight;
    const prefix = this.model.isDoubleDecker ? (this.activeDeck.level === 1 ? 'L' : 'U') : '';
    let seatIdx = 1;

    if (preset === 'standard') {
      this.visualLayout.gridWidth = 5;
      for (let r = 0; r < h - 1; r++) {
        [0, 1, 3, 4].forEach(c => {
          this.activeDeck.seats.push({ row: r, col: c, seatNumber: `${prefix}${seatIdx++}`, seatClassId: this.activeAssignmentClassId });
        });
      }
      [0, 1, 2, 3, 4].forEach(c => {
        this.activeDeck.seats.push({ row: h - 1, col: c, seatNumber: `${prefix}${seatIdx++}`, seatClassId: this.activeAssignmentClassId });
      });
    } else if (preset === 'economy') {
      this.visualLayout.gridWidth = 5;
      for (let r = 0; r < h; r++) {
        [0, 1, 3, 4].forEach(c => {
          this.activeDeck.seats.push({ row: r, col: c, seatNumber: `${prefix}${seatIdx++}`, seatClassId: this.activeAssignmentClassId });
        });
      }
    } else if (preset === 'minibus') {
      this.visualLayout.gridWidth = 4;
      for (let r = 0; r < h; r++) {
        [0, 2, 3].forEach(c => {
          this.activeDeck.seats.push({ row: r, col: c, seatNumber: `${prefix}${seatIdx++}`, seatClassId: this.activeAssignmentClassId });
        });
      }
    }
    this.updateCapacity();
  }
}
