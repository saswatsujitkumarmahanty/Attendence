import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AttendanceService } from '../../core/services/attendance.service';
import { AttendanceRecord } from '../../core/models/attendance.model';

@Component({
 standalone: true,
  imports: [CommonModule, FormsModule],
  selector: 'app-punch',
  styleUrl: './punch.css',
  templateUrl: './punch.html',
})
export class Punch implements OnInit {
  readonly loading = signal(true);
  readonly working = signal(false);
  readonly isPunchedIn = signal(false);
  readonly currentRecord = signal<AttendanceRecord | null>(null);
  readonly notes = signal('');
  readonly errorMessage = signal<string | null>(null);
  readonly infoMessage = signal<string | null>(null);
 
  private employeeId: number | null = null;
 
  constructor(
    private readonly auth: AuthService,
    private readonly attendanceService: AttendanceService
  ) {}
 
  ngOnInit(): void {
    this.employeeId = this.auth.employeeId();
 
    if (!this.employeeId) {
      this.errorMessage.set(
        'Your account is not linked to an employee record, so it cannot punch in or out. Ask an admin to link it.'
      );
      this.loading.set(false);
      return;
    }
 
    this.refreshStatus();
  }
 
  refreshStatus(): void {
    if (!this.employeeId) return;
 
    this.loading.set(true);
    this.attendanceService.getStatus(this.employeeId).subscribe({
      next: (status) => {
        this.isPunchedIn.set(status.isPunchedIn);
        this.currentRecord.set(status.record);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load your current punch status.');
        this.loading.set(false);
      }
    });
  }
 
  punchIn(): void {
    if (!this.employeeId) return;
 
    this.working.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
 
    this.attendanceService.punchIn({ employeeId: this.employeeId, notes: this.notes() || null }).subscribe({
      next: (record) => {
        this.working.set(false);
        this.isPunchedIn.set(true);
        this.currentRecord.set(record);
        this.notes.set('');
        this.infoMessage.set('Punched in successfully.');
      },
      error: (err) => {
        this.working.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Could not punch in.');
      }
    });
  }
 
  punchOut(): void {
    if (!this.employeeId) return;
 
    this.working.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
 
    this.attendanceService.punchOut({ employeeId: this.employeeId, notes: this.notes() || null }).subscribe({
      next: (record) => {
        this.working.set(false);
        this.isPunchedIn.set(false);
        this.currentRecord.set(record);
        this.notes.set('');
        this.infoMessage.set('Punched out successfully.');
      },
      error: (err) => {
        this.working.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Could not punch out.');
      }
    });
  }
}
