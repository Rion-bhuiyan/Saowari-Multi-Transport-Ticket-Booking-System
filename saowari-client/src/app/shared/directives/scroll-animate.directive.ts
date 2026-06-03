import { Directive, ElementRef, OnInit, OnDestroy, Renderer2, Input } from '@angular/core';

@Directive({
  selector: '[appScrollAnimate]',
  standalone: true
})
export class ScrollAnimateDirective implements OnInit, OnDestroy {
  @Input() animationClass: string = 'animate-fade-in-up';
  @Input() delay: string = '0ms';

  private observer: IntersectionObserver | null = null;

  constructor(private el: ElementRef, private renderer: Renderer2) {}

  ngOnInit(): void {
    // Initial state: hide the element before it animates in
    this.renderer.setStyle(this.el.nativeElement, 'opacity', '0');
    if (this.delay !== '0ms') {
      this.renderer.setStyle(this.el.nativeElement, 'animation-delay', this.delay);
    }

    this.observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          // Element is in view, apply animation
          this.renderer.addClass(this.el.nativeElement, this.animationClass);
          this.renderer.setStyle(this.el.nativeElement, 'opacity', '1');
          
          // Stop observing once animated (run once)
          if (this.observer) {
            this.observer.unobserve(this.el.nativeElement);
          }
        }
      });
    }, {
      root: null,
      rootMargin: '0px',
      threshold: 0.1 // Trigger when 10% visible
    });

    this.observer.observe(this.el.nativeElement);
  }

  ngOnDestroy(): void {
    if (this.observer) {
      this.observer.disconnect();
    }
  }
}
