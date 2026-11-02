import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environmentApiUrl } from '../api-config';
import { AttendanceRecord, PunchRequest, PunchStatus } from '../models/attendance.model';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly baseUrl = `${environmentApiUrl}/Attendance`;
  private readonly punchUrl = `${environmentApiUrl}/Punch`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<AttendanceRecord[]> {
    return this.http.get<AttendanceRecord[]>(this.baseUrl);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getStatus(employeeId: number): Observable<PunchStatus> {
    return this.http.get<PunchStatus>(`${this.punchUrl}/status/${employeeId}`);
  }

  punchIn(request: PunchRequest): Observable<AttendanceRecord> {
    return this.http.post<AttendanceRecord>(`${this.punchUrl}/in`, request);
  }

  punchOut(request: PunchRequest): Observable<AttendanceRecord> {
    return this.http.post<AttendanceRecord>(`${this.punchUrl}/out`, request);
  }
}