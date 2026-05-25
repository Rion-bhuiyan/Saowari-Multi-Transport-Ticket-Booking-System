import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationService } from '../services/notification.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private notificationService: NotificationService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMsg = '';
        if (error.error instanceof ErrorEvent) {
          errorMsg = `Error: ${error.error.message}`;
        } else {
          switch (error.status) {
            case 400:
              errorMsg = error.error?.message || 'Bad Request. Please check your input.';
              if (error.error?.errors && Array.isArray(error.error.errors)) {
                errorMsg = error.error.errors.join(', ');
              }
              this.notificationService.warning(errorMsg, 'Validation Error');
              break;
            case 401:
              // Handled by auth interceptor
              break;
            case 403:
              errorMsg = 'Access Denied. You do not have permission.';
              this.notificationService.error(errorMsg, 'Forbidden');
              break;
            case 404:
              errorMsg = 'Requested resource not found.';
              this.notificationService.info(errorMsg, 'Not Found');
              break;
            case 500:
              errorMsg = 'Internal Server Error. Please try again later.';
              this.notificationService.error(errorMsg, 'Server Error');
              break;
            default:
              errorMsg = error.error?.message || `Error Code: ${error.status}\nMessage: ${error.message}`;
              this.notificationService.error(errorMsg, 'Error');
              break;
          }
        }
        return throwError(() => error);
      })
    );
  }
}
