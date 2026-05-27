import sys

file_path = r'd:\Final_project\Saowari\Saowari\saowari-client\src\app\features\booking\booking.component.html'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

start_marker = '<span class="font-mono text-xl font-bold text-saowari-primary tracking-widest">{{ bookingConfirmationCode }}</span>\n              </div>'
end_marker = '              <div class="flex justify-between items-center mb-1">\n                <p class="text-sm text-gray-500">Arrival</p>'

if start_marker not in content:
    print('Start marker not found')
    sys.exit(1)
if end_marker not in content:
    print('End marker not found')
    sys.exit(1)

start_idx = content.find(start_marker) + len(start_marker)
end_idx = content.find(end_marker)

replacement = """
              <div class="divider my-2"></div>
              <div class="grid grid-cols-2 gap-y-3 text-sm">
                <div>
                  <p class="text-gray-400">Journey Date</p>
                  <p class="font-semibold">{{ formatDate(schedule.departureDateTime) }}</p>
                  <p *ngIf="isRoundTrip && returnSchedule" class="font-semibold text-saowari-primary text-xs mt-1">Return: {{ formatDate(returnSchedule.departureDateTime) }}</p>
                </div>
                <div>
                  <p class="text-gray-400">Departure</p>
                  <p class="font-semibold">{{ formatTime(schedule.departureDateTime) }}</p>
                  <p *ngIf="isRoundTrip && returnSchedule" class="font-semibold text-saowari-primary text-xs mt-1">Return: {{ formatTime(returnSchedule.departureDateTime) }}</p>
                </div>
                <div>
                  <p class="text-gray-400">Passengers</p>
                  <p class="font-semibold">{{ seatIds.length }} Person(s)</p>
                </div>
                <div>
                  <p class="text-gray-400">Total Paid</p>
                  <p class="font-bold text-saowari-primary text-lg">৳{{ getFinalFare() }}</p>
                </div>
              </div>
            </div>

            <div class="flex flex-col sm:flex-row gap-4 justify-center">
              <ng-container *ngIf="isRoundTrip && returnConfirmationCode; else singleTicketBtn">
                <button (click)="downloadTicket(outboundConfirmationCode)" class="btn btn-saowari gap-2 px-8">
                  <i class="fas fa-plane-departure"></i> View Outbound Ticket
                </button>
                <button (click)="downloadTicket(returnConfirmationCode)" class="btn btn-saowari gap-2 px-8 bg-blue-700 hover:bg-blue-800 border-none text-white">
                  <i class="fas fa-plane-arrival"></i> View Return Ticket
                </button>
              </ng-container>
              <ng-template #singleTicketBtn>
                <button (click)="downloadTicket(outboundConfirmationCode)" class="btn btn-saowari gap-2 px-8">
                  <i class="fas fa-ticket-alt"></i> View My Ticket
                </button>
              </ng-template>

              <button (click)="goHome()" class="btn btn-outline btn-primary gap-2 px-8 bg-white hover:bg-gray-50 border-gray-300 text-gray-700 hover:border-gray-400 hover:text-gray-900">
                <i class="fas fa-home"></i> Go Home
              </button>
            </div>
          </div>
        </div>

      </div>

      <!-- Sidebar: Order Summary -->
      <div class="lg:col-span-1" *ngIf="currentStep < 3">
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 sticky top-24 overflow-hidden">
          <div class="bg-saowari-primary text-white p-5">
            <h3 class="font-heading font-bold text-lg">Order Summary</h3>
          </div>
          <div class="p-5">
            <div *ngIf="schedule" class="mb-4 pb-4 border-b border-gray-100">
              <h4 *ngIf="isRoundTrip" class="font-bold text-xs uppercase tracking-widest text-saowari-primary mb-2">Outbound</h4>
              <div class="flex justify-between items-center mb-1">
                <p class="text-sm text-gray-500">Departure</p>
                <p class="font-bold text-gray-800">{{ formatTime(schedule.departureDateTime) }}</p>
              </div>
"""

new_content = content[:start_idx] + replacement + content[end_idx:]

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(new_content)

print('Success')
