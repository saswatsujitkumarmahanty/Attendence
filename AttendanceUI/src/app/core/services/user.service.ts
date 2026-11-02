import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environmentApiUrl } from '../api-config';
import { UserListItem } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environmentApiUrl}/Auth`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<UserListItem[]> {
    return this.http.get<UserListItem[]>(`${this.baseUrl}/users`);
  }
}