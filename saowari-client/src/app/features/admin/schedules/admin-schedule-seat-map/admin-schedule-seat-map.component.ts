import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-schedule-seat-map',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-schedule-seat-map.component.html',
  styleUrls: ['./admin-schedule-seat-map.component.css']
})
export class AdminScheduleSeatMapComponent implements OnInit {
  scheduleId!: number;
  isLoading = true;
  seatMapData: any = null;
  seatLayoutConfig: any = null;
  previewSeatMap: any[] = [];
  
  constructor(
    private route: ActivatedRoute,
    private scheduleService: ScheduleService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.scheduleId = Number(idParam);
      this.loadSeatMap();
    }
  }

  loadSeatMap(): void {
    this.isLoading = true;
    this.scheduleService.getSeatMap(this.scheduleId).subscribe({
      next: (res) => {
        this.seatMapData = res;
        if (res.seatLayoutConfig) {
          try {
            this.seatLayoutConfig = JSON.parse(res.seatLayoutConfig);
          } catch(e) {}
        }
        this.generateSeatPreview();
        this.isLoading = false;
      },
      error: (err) => {
        this.notification.error('Failed to load seat map.');
        this.isLoading = false;
      }
    });
  }

  get layoutConfig(): { seatsPerRow: number; aisleAfter: number; backRowSeats: number } {
    const preset = this.seatLayoutConfig?.LayoutPreset || 'standard';
    switch (preset) {
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
      IsDoubleDecker: this.seatLayoutConfig?.IsDoubleDecker || false,
      ContinuousBackRow: this.seatLayoutConfig?.ContinuousBackRow !== false,
      LayoutPreset: this.seatLayoutConfig?.LayoutPreset || 'standard'
    };
  }

  get bookedCount(): number {
    if (!this.seatMapData || !this.seatMapData.seats) return 0;
    return this.seatMapData.seats.filter((s: any) => s.isBooked).length;
  }

  generateSeatPreview() {
    if (!this.seatMapData || !this.seatMapData.seats) return;
    
    const cfg = this.layoutConfig;
    const seatsPerRow = cfg.seatsPerRow;
    const backRowSeats = cfg.backRowSeats;
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const isDoubleDecker = this.seatLayoutConfig?.IsDoubleDecker || false;
    const continuousBackRow = this.seatLayoutConfig?.ContinuousBackRow !== false;
    
    const decks = isDoubleDecker ? 2 : 1;
    const totalCapacity = this.seatMapData.seats.length;
    
    const decksMap = new Map<string, Map<string, any[]>>();
    const allSeats = this.seatMapData.seats;

    let seatIndex = 0;

    for (let d = 1; d <= decks; d++) {
      const capacityForThisDeck = d === 1 ? Math.ceil(totalCapacity / decks) : Math.floor(totalCapacity / decks);
      
      let numRegularRows = 0;
      if (continuousBackRow) {
        numRegularRows = Math.ceil(Math.max(0, capacityForThisDeck - backRowSeats) / seatsPerRow);
      } else {
        numRegularRows = Math.ceil(capacityForThisDeck / seatsPerRow);
      }

      const deckName = isDoubleDecker ? (d === 1 ? 'Lower Deck' : 'Upper Deck') : 'Main Deck';
      let generatedForDeck = 0;
      
      if (!decksMap.has(deckName)) decksMap.set(deckName, new Map());

      for (let r = 0; r < numRegularRows; r++) {
        const rowLetter = r < alphabet.length ? alphabet[r] : `R${r + 1}`;
        if (!decksMap.get(deckName)!.has(rowLetter)) decksMap.get(deckName)!.set(rowLetter, []);

        for (let c = 1; c <= seatsPerRow; c++) {
          if (generatedForDeck >= capacityForThisDeck || seatIndex >= totalCapacity) break;
          decksMap.get(deckName)!.get(rowLetter)!.push(allSeats[seatIndex]);
          seatIndex++;
          generatedForDeck++;
        }
      }

      if (continuousBackRow) {
        const backLetter = numRegularRows < alphabet.length ? alphabet[numRegularRows] : `R${numRegularRows + 1}`;
        if (!decksMap.get(deckName)!.has(backLetter)) decksMap.get(deckName)!.set(backLetter, []);

        for (let c = 1; c <= backRowSeats; c++) {
          if (generatedForDeck >= capacityForThisDeck || seatIndex >= totalCapacity) break;
          decksMap.get(deckName)!.get(backLetter)!.push(allSeats[seatIndex]);
          seatIndex++;
          generatedForDeck++;
        }
      }
    }

    this.previewSeatMap = Array.from(decksMap.entries()).map(([name, rowsMap]) => ({
      deckName: name,
      rows: Array.from(rowsMap.values())
    }));
  }

  getProfilePictureUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
      return path;
    }
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return 'http://localhost:5293' + cleanPath;
  }

  isLicenseExpired(dateStr: string | null | undefined): boolean {
    if (!dateStr) return false;
    return new Date(dateStr) < new Date();
  }
}
