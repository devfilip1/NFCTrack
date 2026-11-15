import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateLinkRequest, Link, LinkStats, UpdateLinkRequest } from './link.models';

@Injectable({ providedIn: 'root' })
export class LinksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/links';

  list(): Observable<Link[]> {
    return this.http.get<Link[]>(this.baseUrl);
  }

  get(id: string): Observable<Link> {
    return this.http.get<Link>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateLinkRequest): Observable<Link> {
    return this.http.post<Link>(this.baseUrl, request);
  }

  updateDestination(id: string, request: UpdateLinkRequest): Observable<Link> {
    return this.http.put<Link>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  stats(id: string, days: number): Observable<LinkStats> {
    return this.http.get<LinkStats>(`${this.baseUrl}/${id}/stats`, { params: { days } });
  }
}
