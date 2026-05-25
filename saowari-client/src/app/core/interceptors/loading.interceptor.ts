import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Exclude certain endpoints from loading spinner
    if (request.url.includes('/auth/refresh-token') || request.url.includes('/dashboard/summary')) {
      return next.handle(request);
    }

    this.loadingService.show();
    return next.handle(request).pipe(
      finalize(() => this.loadingService.hide())
    );
  }
}
