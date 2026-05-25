import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title class="font-heading font-semibold">{{ data.title }}</h2>
    <mat-dialog-content>
      <p class="text-saowari-text-secondary">{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end" class="pb-4 pr-4">
      <button mat-button mat-dialog-close>{{ data.cancelLabel || 'Cancel' }}</button>
      <button mat-raised-button 
              [color]="data.isDanger ? 'warn' : 'primary'" 
              [mat-dialog-close]="true">
        {{ data.confirmLabel || 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}
}
