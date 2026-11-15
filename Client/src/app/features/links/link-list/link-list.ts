import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { Link } from '../link.models';

@Component({
  imports: [],
  selector: 'app-link-list',
  styleUrl: './link-list.scss',
  templateUrl: './link-list.html',
})
export class LinkList {
  protected readonly links = toSignal(inject(HttpClient).get<Link[]>('/api/links'), { initialValue: [] });
}
