import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';

const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { 
        path: 'home', 
        loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) 
      },
      { 
        path: 'search', 
        loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent) 
      },
      { 
        path: 'schedules/:id', 
        loadComponent: () => import('./features/schedules/schedule-detail/schedule-detail.component').then(m => m.ScheduleDetailComponent) 
      },
      { 
        path: 'booking', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/booking/booking.component').then(m => m.BookingComponent) 
      },
      {
        path: 'about',
        loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent)
      },
      {
        path: 'contact',
        loadComponent: () => import('./features/contact/contact.component').then(m => m.ContactComponent)
      },
      {
        path: 'faq',
        loadComponent: () => import('./features/faq/faq.component').then(m => m.FaqComponent)
      },
      {
        path: 'legal',
        loadComponent: () => import('./features/legal/legal.component').then(m => m.LegalComponent)
      },
      { 
        path: 'profile/dashboard', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/profile-dashboard/profile-dashboard.component').then(m => m.ProfileDashboardComponent) 
      },
      { 
        path: 'profile/my-bookings', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/my-bookings/my-bookings.component').then(m => m.MyBookingsComponent) 
      },
      { 
        path: 'profile/my-tickets', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/my-tickets/my-tickets.component').then(m => m.MyTicketsComponent) 
      },
      { 
        path: 'profile/my-refunds', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/my-refunds/my-refunds.component').then(m => m.MyRefundsComponent) 
      },
      { 
        path: 'profile/edit-profile', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/edit-profile/edit-profile.component').then(m => m.EditProfileComponent) 
      },
      { 
        path: 'profile/change-password', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/change-password/change-password.component').then(m => m.ChangePasswordComponent) 
      },
      { 
        path: 'profile/schedule-chats', 
        canActivate: [AuthGuard],
        loadComponent: () => import('./features/profile/schedule-chats/schedule-chats.component').then(m => m.ScheduleChatsComponent) 
      }
    ]
  },
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { 
        path: 'login', 
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) 
      },
      { 
        path: 'register', 
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) 
      },
      { 
        path: 'forgot-password', 
        loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) 
      },
      { 
        path: 'reset-password', 
        component: ResetPasswordComponent
      }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [AuthGuard, AdminGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { 
        path: 'dashboard', 
        loadComponent: () => import('./features/admin/dashboard/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) 
      },
      { 
        path: 'manager-dashboard', 
        loadComponent: () => import('./features/admin/dashboard/company-manager-dashboard/company-manager-dashboard.component').then(m => m.CompanyManagerDashboardComponent) 
      },
      { 
        path: 'supervisor-dashboard', 
        loadComponent: () => import('./features/admin/dashboard/supervisor-dashboard/supervisor-dashboard.component').then(m => m.SupervisorDashboardComponent) 
      },
      { 
        path: 'driver-dashboard', 
        loadComponent: () => import('./features/admin/dashboard/driver-dashboard/driver-dashboard.component').then(m => m.DriverDashboardComponent) 
      },
      { 
        path: 'users', 
        loadComponent: () => import('./features/admin/users/admin-users/admin-users.component').then(m => m.AdminUsersComponent) 
      },
      { 
        path: 'users/:id', 
        loadComponent: () => import('./features/admin/users/admin-user-details/admin-user-details.component').then(m => m.AdminUserDetailsComponent) 
      },
      { 
        path: 'companies', 
        loadComponent: () => import('./features/admin/companies/admin-companies/admin-companies.component').then(m => m.AdminCompaniesComponent) 
      },
      { 
        path: 'locations', 
        loadComponent: () => import('./features/admin/locations/admin-locations/admin-locations.component').then(m => m.AdminLocationsComponent) 
      },
      { 
        path: 'sliders', 
        loadComponent: () => import('./features/admin/slider-images/admin-sliders/admin-sliders.component').then(m => m.AdminSlidersComponent) 
      },
      { 
        path: 'banners', 
        loadComponent: () => import('./features/admin/banners/admin-banners.component').then(m => m.AdminBannersComponent) 
      },
      { 
        path: 'vehicles', 
        loadComponent: () => import('./features/admin/vehicles/admin-vehicles/admin-vehicles.component').then(m => m.AdminVehiclesComponent) 
      },
      {
        path: 'vehicle-types',
        loadComponent: () => import('./features/admin/vehicles/admin-vehicle-types/admin-vehicle-types.component').then(m => m.AdminVehicleTypesComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/admin/roles/admin-roles/admin-roles.component').then(m => m.AdminRolesComponent)
      },
      {
        path: 'seat-classes',
        loadComponent: () => import('./features/admin/seat-classes/admin-seat-classes/admin-seat-classes.component').then(m => m.AdminSeatClassesComponent)
      },
      { 
        path: 'routes', 
        loadComponent: () => import('./features/admin/routes/admin-routes/admin-routes.component').then(m => m.AdminRoutesComponent) 
      },
      { 
        path: 'schedules', 
        loadComponent: () => import('./features/admin/schedules/admin-schedules/admin-schedules.component').then(m => m.AdminSchedulesComponent) 
      },
      {
        path: 'schedule-lifecycle',
        loadComponent: () => import('./features/admin/schedules/schedule-lifecycle/schedule-lifecycle.component').then(m => m.ScheduleLifecycleComponent)
      },
      {
        path: 'schedules/:id/seat-map',
        loadComponent: () => import('./features/admin/schedules/admin-schedule-seat-map/admin-schedule-seat-map.component').then(m => m.AdminScheduleSeatMapComponent)
      },
      { 
        path: 'bookings', 
        loadComponent: () => import('./features/admin/bookings/admin-bookings/admin-bookings.component').then(m => m.AdminBookingsComponent) 
      },
      { 
        path: 'payments', 
        loadComponent: () => import('./features/admin/payments/admin-payments/admin-payments.component').then(m => m.AdminPaymentsComponent) 
      },
      { 
        path: 'payment-methods', 
        loadComponent: () => import('./features/admin/payments/admin-payment-methods/admin-payment-methods.component').then(m => m.AdminPaymentMethodsComponent) 
      },
      { 
        path: 'refunds', 
        loadComponent: () => import('./features/admin/refunds/admin-refunds/admin-refunds.component').then(m => m.AdminRefundsComponent) 
      },
      { 
        path: 'discounts', 
        loadComponent: () => import('./features/admin/discounts/admin-discounts/admin-discounts.component').then(m => m.AdminDiscountsComponent) 
      },
      { 
        path: 'reports', 
        loadComponent: () => import('./features/admin/reports/admin-reports/admin-reports.component').then(m => m.AdminReportsComponent) 
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/admin/notifications/admin-notifications/admin-notifications.component').then(m => m.AdminNotificationsComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/admin/settings/admin-settings/admin-settings.component').then(m => m.AdminSettingsComponent)
      },
      {
        path: 'messenger',
        loadComponent: () => import('./features/admin/messenger/admin-messenger.component').then(m => m.AdminMessengerComponent)
      },
      {
        path: 'broadcast',
        loadComponent: () => import('./features/admin/broadcast/broadcast.component').then(m => m.BroadcastComponent)
      }
    ]
  },
  {
    path: 'ticket/:id',
    loadComponent: () => import('./features/booking/ticket-view/ticket-view.component').then(m => m.TicketViewComponent)
  },
  { path: '**', redirectTo: '/home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'enabled' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
