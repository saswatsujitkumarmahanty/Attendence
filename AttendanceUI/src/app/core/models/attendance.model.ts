export interface AttendanceRecord {
  attendanceId: number;
  employeeId: number;
  attendanceDate: string;
  status: string;
  punchInTime: string | null;
  punchOutTime: string | null;
  notes: string | null;
  markedBy: number;
}

export interface PunchRequest {
  employeeId: number;
  notes?: string | null;
}

export interface PunchStatus {
  isPunchedIn: boolean;
  record: AttendanceRecord | null;
}