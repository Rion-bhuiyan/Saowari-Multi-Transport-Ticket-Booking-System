import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { QRCodeModule } from 'angularx-qrcode';
import { environment } from '../../../../environments/environment';
import { NotificationService } from '../../../core/services/notification.service';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

@Component({
  selector: 'app-ticket-view',
  standalone: true,
  imports: [CommonModule, RouterModule, QRCodeModule],
  providers: [DatePipe],
  templateUrl: './ticket-view.component.html',
  styleUrls: ['./ticket-view.component.css']
})
export class TicketViewComponent implements OnInit {
  @ViewChild('ticketElement', { static: false }) ticketElement!: ElementRef;

  ticketId: string | null = null;
  ticketData: any = null;
  isLoading = true;
  qrCodeData = '';
  isGeneratingPdf = false;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id');
    if (this.ticketId) {
      this.qrCodeData = `${window.location.origin}/ticket/${this.ticketId}`;
      this.fetchTicketDetails();
    } else {
      this.isLoading = false;
      this.notification.error('Invalid Ticket ID');
    }
  }

  fetchTicketDetails() {
    // If the param is purely numeric, use the old endpoint; otherwise treat as a slug
    const isNumeric = /^\d+$/.test(this.ticketId!);
    const apiUrl = isNumeric
      ? `${environment.apiUrl}/bookings/${this.ticketId}/ticket`
      : `${environment.apiUrl}/bookings/code/${this.ticketId}/ticket`;

    this.http.get<any>(apiUrl).subscribe({
      next: (res) => {
        if (res.success) {
          this.ticketData = res.data;
          // Update QR to always show the canonical slug URL once we have the data
          if (!isNumeric && this.ticketData?.bookingCode) {
            const nameSlug = (this.ticketData.passengerName || 'Passenger')
              .replace(/\s+/g, '')
              .replace(/[^a-zA-Z0-9]/g, '');
            this.qrCodeData = `${window.location.origin}/ticket/${this.ticketData.bookingCode}-${nameSlug}`;
          }
        } else {
          this.notification.error(res.message || 'Failed to load ticket details.');
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.notification.error('Error fetching ticket details.');
        this.isLoading = false;
      }
    });
  }

  printTicket() {
    window.print();
  }

  async downloadPdf() {
    if (this.isGeneratingPdf) return;
    this.isGeneratingPdf = true;
    this.notification.success('Generating PDF, please wait...');

    // Small delay to let the notification render and ensure DOM is stable
    await new Promise(r => setTimeout(r, 300));

    const element = this.ticketElement.nativeElement;

    try {
      const canvas = await html2canvas(element, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        logging: false,
        backgroundColor: '#ffffff',
        imageTimeout: 8000,
        scrollX: 0,
        scrollY: 0,
        onclone: (clonedDoc) => {
          // In the cloned document, convert any <canvas> QR codes to <img> if missed
          const canvases = clonedDoc.querySelectorAll('canvas');
          canvases.forEach((c: HTMLCanvasElement) => {
            const img = clonedDoc.createElement('img');
            img.src = c.toDataURL('image/png');
            img.style.width = c.style.width || c.width + 'px';
            img.style.height = c.style.height || c.height + 'px';
            c.parentNode?.replaceChild(img, c);
          });
        }
      });

      const imgData = canvas.toDataURL('image/png');

      // A4 size: 210mm × 297mm
      const pdf = new jsPDF('p', 'mm', 'a4');
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const pdfHeight = pdf.internal.pageSize.getHeight();

      let imgWidth = pdfWidth;
      let imgHeight = (canvas.height * pdfWidth) / canvas.width;

      // If the content height exceeds A4 height, scale down to fit exactly on 1 page
      if (imgHeight > pdfHeight) {
        const ratio = pdfHeight / imgHeight;
        imgWidth = imgWidth * ratio;
        imgHeight = pdfHeight;
      }

      // Place exactly at the top-left to eliminate top white space
      const xOffset = (pdfWidth - imgWidth) / 2;
      const yOffset = 0;

      pdf.addImage(imgData, 'PNG', xOffset, yOffset, imgWidth, imgHeight);
      pdf.save(`Ticket_${this.ticketData?.bookingCode || this.ticketId}.pdf`);
      this.notification.success('Ticket downloaded successfully!');
    } catch (err) {
      console.error('PDF generation error:', err);
      this.notification.error('Failed to generate PDF. Please use the Print button instead.');
    } finally {
      this.isGeneratingPdf = false;
    }
  }

  onImgError(event: Event) {
    const el = event.target as HTMLImageElement;
    if (el) el.style.display = 'none';
  }
}
