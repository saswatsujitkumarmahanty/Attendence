import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { AttendanceService } from '../../../core/services/attendance.service';
import { EmployeeService } from '../../../core/services/employee.service';
import { UserService } from '../../../core/services/user.service';
import { AttendanceRecord } from '../../../core/models/attendance.model';
import { Employee } from '../../../core/models/employee.model';
import { UserListItem } from '../../../core/models/user.model';

@Component({
  standalone: true,
  imports: [CommonModule],
  selector: 'app-attendance-list',
  styleUrl: './attendance-list.css',
  templateUrl: './attendance-list.html',
})
export class AttendanceList implements OnInit {
  readonly records = signal<AttendanceRecord[]>([]);
  readonly employeesById = signal<Map<number, Employee>>(new Map());
  readonly usernameByEmployeeId = signal<Map<number, string>>(new Map());
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly attendanceService: AttendanceService,
    private readonly employeeService: EmployeeService,
    private readonly userService: UserService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      employees: this.employeeService.getAll(),
      users: this.userService.getAll()
    }).subscribe({
      next: ({ employees, users }) => {
        this.employeesById.set(new Map(employees.map((e) => [e.employeeId, e])));
        this.usernameByEmployeeId.set(this.buildUsernameMap(users));

        this.attendanceService.getAll().subscribe({
          next: (records) => {
            this.records.set(records);
            this.loading.set(false);
          },
          error: () => {
            this.errorMessage.set('Could not load attendance records.');
            this.loading.set(false);
          }
        });
      },
      error: () => {
        this.errorMessage.set('Could not load employees.');
        this.loading.set(false);
      }
    });
  }

  employeeLabel(employeeId: number): string {
    const username = this.usernameByEmployeeId().get(employeeId);
    if (username) {
      return username;
    }

    const employee = this.employeesById().get(employeeId);
    return employee ? employee.employeeName : `#${employeeId}`;
  }

  remove(record: AttendanceRecord): void {
    if (!confirm('Delete this attendance record?')) {
      return;
    }

    this.attendanceService.delete(record.attendanceId).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('Could not delete that record.')
    });
  }

  downloadCsv(): void {
  const rows = this.records();
  if (!rows.length) return;

  const header = ['Name', 'Date', 'Status', 'Punch In', 'Punch Out', 'Notes'];

  const lines = rows.map((rec) => [
    this.employeeLabel(rec.employeeId),
    new Date(rec.attendanceDate).toLocaleDateString(),
    rec.status,
    rec.punchInTime ? new Date(rec.punchInTime).toLocaleString() : '',
    rec.punchOutTime ? new Date(rec.punchOutTime).toLocaleString() : '',
    rec.notes ?? ''
  ]);

  const csv = [header, ...lines]
    .map((row) => row.map((cell) => this.escapeCsvCell(cell)).join(','))
    .join('\r\n');

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);

  const link = document.createElement('a');
  link.href = url;
  link.download = `attendance-records-${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();

  URL.revokeObjectURL(url);
}

private escapeCsvCell(value: string): string {
  if (value.includes(',') || value.includes('"') || value.includes('\n')) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

  private buildUsernameMap(users: UserListItem[]): Map<number, string> {
  const map = new Map<number, string>();

  for (const user of users) {
    if (user.employeeId !== null) {
      map.set(user.employeeId, user.username);
    }
  }

  return map;
}
}