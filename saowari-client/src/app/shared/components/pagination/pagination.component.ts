import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class PaginationComponent implements OnChanges {
  @Input() totalItems: number = 0;
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 15;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  totalPages: number = 1;
  pages: (number | '...')[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    this.calculate();
  }

  calculate() {
    this.totalPages = Math.max(1, Math.ceil(this.totalItems / this.pageSize));
    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }
    this.pages = this.buildPages();
  }

  buildPages(): (number | '...')[] {
    const total = this.totalPages;
    const cur = this.currentPage;
    const delta = 2;
    const range: number[] = [];
    const result: (number | '...')[] = [];

    for (let i = Math.max(2, cur - delta); i <= Math.min(total - 1, cur + delta); i++) {
      range.push(i);
    }

    if (total <= 1) return [1];

    result.push(1);
    if (range[0] > 2) result.push('...');
    result.push(...range);
    if (range[range.length - 1] < total - 1) result.push('...');
    if (total > 1) result.push(total);

    return result;
  }

  goTo(page: number | '...') {
    if (page === '...' || page === this.currentPage) return;
    const p = page as number;
    if (p < 1 || p > this.totalPages) return;
    this.currentPage = p;
    this.pages = this.buildPages();
    this.pageChange.emit(p);
  }

  prev() {
    if (this.currentPage > 1) this.goTo(this.currentPage - 1);
  }

  next() {
    if (this.currentPage < this.totalPages) this.goTo(this.currentPage + 1);
  }

  onSizeChange() {
    this.currentPage = 1;
    this.calculate();
    this.pageSizeChange.emit(this.pageSize);
    this.pageChange.emit(1);
  }

  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}
