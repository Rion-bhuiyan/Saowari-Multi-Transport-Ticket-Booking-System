import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SliderImageService } from '../../../../core/services/api/slider-image.service';
import { SliderImageModel } from '../../../../core/models/master.model';

@Component({
  selector: 'app-home-slider',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-slider.component.html',
  styleUrls: ['./home-slider.component.css']
})
export class HomeSliderComponent implements OnInit, OnDestroy {
  slides: SliderImageModel[] = [];
  currentIndex = 0;
  private autoplayIntervalId: any = null;

  // Curated premium fallback slides if DB is empty
  fallbackSlides: SliderImageModel[] = [
    {
      sliderImageID: -1,
      imageUrl: 'assets/images/hero-banner.png',
      title: 'Travel Smarter, Book Faster',
      subtitle: 'Find and book Bus, Launch & Flight tickets across Bangladesh in seconds.',
      displayOrder: 1,
      isActive: true
    },
    {
      sliderImageID: -2,
      imageUrl: 'https://images.unsplash.com/photo-1544735716-392fe2489ffa?auto=format&fit=crop&w=1200&q=80',
      title: 'Explore Cox\'s Bazar',
      subtitle: 'Premium Bus services starting from only ৳900. Book your seats today!',
      displayOrder: 2,
      isActive: true
    },
    {
      sliderImageID: -3,
      imageUrl: 'https://images.unsplash.com/photo-1608958416744-8cb3b8275fcf?auto=format&fit=crop&w=1200&q=80',
      title: 'Launch Journeys in Luxury',
      subtitle: 'Experience the scenic rivers of Barisal with high-end cabin bookings.',
      displayOrder: 3,
      isActive: true
    }
  ];

  constructor(private svc: SliderImageService) {}

  ngOnInit(): void {
    this.loadSlides();
  }

  ngOnDestroy(): void {
    this.stopAutoplay();
  }

  loadSlides() {
    this.svc.getActive().subscribe({
      next: (res: any) => {
        if (res.success && res.data && res.data.length > 0) {
          this.slides = res.data;
        } else {
          this.slides = this.fallbackSlides;
        }
        this.startAutoplay();
      },
      error: () => {
        this.slides = this.fallbackSlides;
        this.startAutoplay();
      }
    });
  }

  startAutoplay() {
    this.stopAutoplay();
    this.autoplayIntervalId = setInterval(() => {
      this.next();
    }, 6000); // Auto slide every 6 seconds
  }

  stopAutoplay() {
    if (this.autoplayIntervalId) {
      clearInterval(this.autoplayIntervalId);
      this.autoplayIntervalId = null;
    }
  }

  prev() {
    this.currentIndex = (this.currentIndex === 0) 
      ? this.slides.length - 1 
      : this.currentIndex - 1;
    this.resetAutoplay();
  }

  next() {
    this.currentIndex = (this.currentIndex === this.slides.length - 1) 
      ? 0 
      : this.currentIndex + 1;
    this.resetAutoplay();
  }

  goToSlide(index: number) {
    this.currentIndex = index;
    this.resetAutoplay();
  }

  private resetAutoplay() {
    this.startAutoplay();
  }

  onMouseEnter() {
    this.stopAutoplay();
  }

  onMouseLeave() {
    this.startAutoplay();
  }
}
