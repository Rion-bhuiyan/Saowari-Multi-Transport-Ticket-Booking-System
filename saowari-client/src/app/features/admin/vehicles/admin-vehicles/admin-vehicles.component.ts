import { Component, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-admin-vehicles',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-vehicles.component.html',
  styleUrls: ['./admin-vehicles.component.css']
})
export class AdminVehiclesComponent implements OnInit {
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
  layoutModel: any = {
    isDoubleDecker: false,
    continuousBackRow: true
  };
  model: any = { 
    vehicleName: '', vehicleNumber: '', companyId: null, vehicleTypeId: null, 
    engineNumber: '', engineCC: '', chassisNumber: '',
    capacity: 40, isActive: true,
    isDoubleDecker: false, continuousBackRow: true,
    layoutPreset: 'standard'
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
      let continuousBackRow = true;
      let layoutPreset = 'standard';
      if (item.seatLayoutConfig) {
        try {
          const config = JSON.parse(item.seatLayoutConfig);
          isDoubleDecker = config.IsDoubleDecker || false;
          continuousBackRow = config.ContinuousBackRow !== false; // default true if undefined
          layoutPreset = config.LayoutPreset || 'standard';
        } catch(e) {}
      }

      this.model = { 
        ...item,
        capacity: item.totalSeats,
        isDoubleDecker,
        continuousBackRow,
        layoutPreset
      };

      if (item.seats) {
        item.seats.forEach((seat: any) => {
          this.seatClassAssignments[seat.seatNumber] = seat.seatClassId;
        });
      }

      this.activeAssignmentClassId = this.seatClasses[0]?.seatClassId || 1;
    } else {
      this.editingItem = null;
      this.model = { 
        vehicleName: '', vehicleNumber: '', companyId: null, vehicleTypeId: null, 
        engineNumber: '', engineCC: '', chassisNumber: '',
        capacity: 40, isActive: true,
        isDoubleDecker: false, continuousBackRow: true,
        layoutPreset: 'standard'
      };
      this.activeAssignmentClassId = this.seatClasses[0]?.seatClassId || 1;
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  getSeatClassId(seatNumber: string): number {
    return this.seatClassAssignments[seatNumber] || this.seatClasses[0]?.seatClassId || 1;
  }

  getSeatClassNameShort(seatNumber: string): string {
    const classId = this.getSeatClassId(seatNumber);
    const sc = this.seatClasses.find(c => c.seatClassId === classId);
    if (!sc) return 'ECO';
    return sc.seatClassName.substring(0, 3).toUpperCase();
  }

  getSeatClassColor(seatNumber: string): string {
    const classId = this.getSeatClassId(seatNumber);
    const sc = this.seatClasses.find(c => c.seatClassId === classId);
    if (!sc) return 'bg-white border-gray-300 text-gray-600';
    const name = sc.seatClassName.toLowerCase();
    if (name.includes('business')) return 'bg-indigo-50 border-indigo-300 text-indigo-700 hover:bg-indigo-100';
    if (name.includes('economy')) return 'bg-slate-50 border-slate-300 text-slate-700 hover:bg-slate-100';
    if (name.includes('first') || name.includes('ac')) return 'bg-emerald-50 border-emerald-300 text-emerald-700 hover:bg-emerald-100';
    if (name.includes('sleeper')) return 'bg-purple-50 border-purple-300 text-purple-700 hover:bg-purple-100';
    return 'bg-amber-50 border-amber-300 text-amber-700 hover:bg-amber-100';
  }

  assignSeatClass(seatNumber: string) {
    this.seatClassAssignments[seatNumber] = this.activeAssignmentClassId;
  }

  assignAllSeats(classId: number) {
    if (!classId) return;
    const seatMap = this.previewSeatMap;
    seatMap.forEach(deck => {
      deck.rows.forEach(row => {
        row.forEach((seat: any) => {
          this.seatClassAssignments[seat.seatNumber] = classId;
        });
      });
    });
    this.notification.success('All seats assigned to ' + (this.seatClasses.find(c => c.seatClassId === classId)?.seatClassName || 'selected class'));
  }

  save() {
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
      continuousBackRow: this.model.continuousBackRow,
      layoutPreset: this.model.layoutPreset,
      isActive: this.model.isActive
    };

    const request = this.editingItem 
      ? this.svc.update(this.editingItem.vehicleID || this.editingItem.vehicleId || this.editingItem.id, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          const savedVehicleId = res.data?.vehicleID || res.data?.vehicleId || res.data?.id || this.editingItem?.vehicleID || this.editingItem?.vehicleId || this.editingItem?.id;
          
          const assignmentsPayload: any[] = [];
          const seatMap = this.previewSeatMap;
          seatMap.forEach(deck => {
            deck.rows.forEach(row => {
              row.forEach((seat: any) => {
                const classId = this.getSeatClassId(seat.seatNumber);
                assignmentsPayload.push({
                  seatNumber: seat.seatNumber,
                  seatClassId: classId
                });
              });
            });
          });

          if (savedVehicleId && assignmentsPayload.length > 0) {
            this.svc.updateSeatClasses(savedVehicleId, assignmentsPayload).subscribe({
              next: () => {
                this.notification.success(this.editingItem ? 'Vehicle and seat classes updated successfully!' : 'Vehicle and seat classes configured successfully!');
                this.closeModal();
                this.load();
              },
              error: () => {
                this.notification.warning('Vehicle saved, but failed to save seat class assignments.');
                this.closeModal();
                this.load();
              }
            });
          } else {
            this.notification.success(this.editingItem ? 'Vehicle updated successfully!' : 'Vehicle created successfully!');
            this.closeModal();
            this.load();
          }
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

  selectLayout(preset: string) {
    this.model.layoutPreset = preset;
    switch (preset) {
      case 'standard':
        this.model.isDoubleDecker = false;
        this.model.continuousBackRow = false;
        break;
      case 'economy':
        this.model.isDoubleDecker = false;
        this.model.continuousBackRow = false;
        break;
      case 'standard-back':
        this.model.isDoubleDecker = false;
        this.model.continuousBackRow = true;
        break;
      case 'double-decker':
        this.model.isDoubleDecker = true;
        this.model.continuousBackRow = true;
        break;
      case 'sleeper':
        this.model.isDoubleDecker = false;
        this.model.continuousBackRow = false;
        break;
      case 'minibus':
        this.model.isDoubleDecker = false;
        this.model.continuousBackRow = false;
        break;
    }
  }

  get layoutInfo(): { name: string; icon: string; seating: string; detail: string; color: string } {
    switch (this.model.layoutPreset) {
      case 'standard':
        return { name: 'Standard 1+2', icon: 'fa-bus', seating: '1 seat ╠═══ Aisle ═══╣ 2 seats per row', detail: '3 columns · Aisle after column 1 · Seat prefix: A1, A2, A3…', color: 'text-gray-700' };
      case 'economy':
        return { name: 'Economy 2+2', icon: 'fa-bus-alt', seating: '2 seats ╠═══ Aisle ═══╣ 2 seats per row', detail: '4 columns · Aisle after column 2 · Seat prefix: A1, A2, A3, A4…', color: 'text-gray-700' };
      case 'standard-back':
        return { name: 'Standard 1+2 + Back Row', icon: 'fa-bus', seating: '1 seat ╠═══ Aisle ═══╣ 2 seats per row + Full rear bench', detail: '3 columns + back row of 4 seats spanning full width', color: 'text-blue-700' };
      case 'double-decker':
        return { name: 'Double Decker', icon: 'fa-building', seating: '1 seat ╠═ Aisle ═╣ 2 seats · Two floors', detail: 'Lower Deck: L-prefix (LA1, LA2…) · Upper Deck: U-prefix (UA1, UA2…)', color: 'text-indigo-700' };
      case 'sleeper':
        return { name: 'Sleeper Berth 2+2', icon: 'fa-bed', seating: '2 wide berths ╠══ Aisle ══╣ 2 wide berths per row', detail: '4 wide columns · Luxury recliner/sleeper configuration · No back row', color: 'text-purple-700' };
      case 'minibus':
        return { name: 'Minibus 1+1', icon: 'fa-shuttle-van', seating: '1 seat ╠═══ Aisle ═══╣ 1 seat per row', detail: '2 narrow columns · Compact minibus layout · Seat prefix: A1, A2…', color: 'text-orange-700' };
      default:
        return { name: 'Standard', icon: 'fa-bus', seating: '1 seat | Aisle | 2 seats per row', detail: '', color: 'text-gray-700' };
    }
  }

  // Returns config based on selected layout preset
  get layoutConfig(): { seatsPerRow: number; aisleAfter: number; backRowSeats: number } {
    switch (this.model.layoutPreset) {
      case 'economy':    return { seatsPerRow: 4, aisleAfter: 2, backRowSeats: 5 };
      case 'standard-back': return { seatsPerRow: 3, aisleAfter: 1, backRowSeats: 4 };
      case 'double-decker': return { seatsPerRow: 3, aisleAfter: 1, backRowSeats: 4 };
      case 'sleeper':    return { seatsPerRow: 4, aisleAfter: 2, backRowSeats: 5 };
      case 'minibus':    return { seatsPerRow: 2, aisleAfter: 1, backRowSeats: 3 };
      default:           return { seatsPerRow: 3, aisleAfter: 1, backRowSeats: 4 }; // standard
    }
  }

  get seatPreviewConfig() {
    const cfg = this.layoutConfig;
    return {
      Columns: cfg.seatsPerRow,
      AisleAfterColumn: cfg.aisleAfter,
      IsDoubleDecker: this.model.isDoubleDecker,
      ContinuousBackRow: this.model.continuousBackRow
    };
  }

  get previewSeatMap() {
    const cfg = this.layoutConfig;
    const seatsPerRow = cfg.seatsPerRow;
    const backRowSeats = cfg.backRowSeats;
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const decks = this.model.isDoubleDecker ? 2 : 1;
    const totalCapacity = this.model.capacity || 0;
    
    const decksMap = new Map<string, Map<string, any[]>>();

    for (let d = 1; d <= decks; d++) {
      const capacityForThisDeck = d === 1 ? Math.ceil(totalCapacity / decks) : Math.floor(totalCapacity / decks);
      
      let numRegularRows = 0;
      if (this.model.continuousBackRow) {
        numRegularRows = Math.ceil(Math.max(0, capacityForThisDeck - backRowSeats) / seatsPerRow);
      } else {
        numRegularRows = Math.ceil(capacityForThisDeck / seatsPerRow);
      }

      const deckName = this.model.isDoubleDecker ? (d === 1 ? 'Lower Deck' : 'Upper Deck') : 'Main Deck';
      const deckPrefix = this.model.isDoubleDecker ? (d === 1 ? 'L' : 'U') : '';
      let generatedForDeck = 0;
      
      if (!decksMap.has(deckName)) decksMap.set(deckName, new Map());

      for (let r = 0; r < numRegularRows; r++) {
        const rowLetter = r < alphabet.length ? alphabet[r] : `R${r + 1}`;
        if (!decksMap.get(deckName)!.has(rowLetter)) decksMap.get(deckName)!.set(rowLetter, []);

        for (let c = 1; c <= seatsPerRow; c++) {
          if (generatedForDeck >= capacityForThisDeck) break;
          decksMap.get(deckName)!.get(rowLetter)!.push({
            seatNumber: `${deckPrefix}${rowLetter}${c}`
          });
          generatedForDeck++;
        }
      }

      if (this.model.continuousBackRow) {
        const backLetter = numRegularRows < alphabet.length ? alphabet[numRegularRows] : `R${numRegularRows + 1}`;
        if (!decksMap.get(deckName)!.has(backLetter)) decksMap.get(deckName)!.set(backLetter, []);

        for (let c = 1; c <= backRowSeats; c++) {
          if (generatedForDeck >= capacityForThisDeck) break;
          decksMap.get(deckName)!.get(backLetter)!.push({
            seatNumber: `${deckPrefix}${backLetter}${c}`
          });
          generatedForDeck++;
        }
      }
    }

    return Array.from(decksMap.entries()).map(([name, rowsMap]) => ({
      deckName: name,
      rows: Array.from(rowsMap.values())
    }));
  }
}
