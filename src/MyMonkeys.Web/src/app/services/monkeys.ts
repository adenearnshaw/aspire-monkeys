import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Monkey {
  id: string;
  name: string;
  location: string;
  details: string;
  imageUrl: string;
  population: number;
  latitude: number;
  longitude: number;
}

@Injectable({
  providedIn: 'root',
})
export class Monkeys {
  private readonly baseUrl = '/api';

  constructor(private readonly http: HttpClient) {}

  list(): Observable<Monkey[]> {
    return this.http.get<Monkey[]>(`${this.baseUrl}/monkeys`);
  }

  get(id: string): Observable<Monkey> {
    return this.http.get<Monkey>(`${this.baseUrl}/monkeys/${encodeURIComponent(id)}`);
  }
}
