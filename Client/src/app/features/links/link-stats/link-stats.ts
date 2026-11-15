import { Clipboard } from '@angular/cdk/clipboard';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { DailyClicks } from '../link.models';
import { LinksService } from '../links.service';
import { shortUrl } from '../short-url';
import { ClicksChart } from './clicks-chart/clicks-chart';

@Component({
  selector: 'app-link-stats',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    ClicksChart,
  ],
  templateUrl: './link-stats.html',
  styleUrl: './link-stats.scss',
})
export class LinkStats {
  private readonly linksService = inject(LinksService);
  private readonly clipboard = inject(Clipboard);
  private readonly snackBar = inject(MatSnackBar);

  /** Bound from the `:id` route parameter (see withComponentInputBinding in app.config). */
  readonly id = input.required<string>();

  protected readonly periods = [7, 30, 90];
  protected readonly days = signal(30);

  protected readonly link = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.linksService.get(id),
  });

  protected readonly stats = rxResource({
    params: () => ({ id: this.id(), days: this.days() }),
    stream: ({ params }) => this.linksService.stats(params.id, params.days),
  });

  protected readonly shortUrl = computed(() => {
    const link = this.link.value();
    return link ? shortUrl(link) : '';
  });

  protected readonly periodClicks = computed(
    () => this.stats.value()?.daily.reduce((sum, day) => sum + day.clicks, 0) ?? 0,
  );

  protected readonly busiestDay = computed(() => {
    const daily = this.stats.value()?.daily ?? [];
    return daily.reduce<DailyClicks | null>(
      (top, day) => (day.clicks > (top?.clicks ?? 0) ? day : top),
      null,
    );
  });

  protected readonly activeDays = computed(
    () =>
      this.stats
        .value()
        ?.daily.filter((day) => day.clicks > 0)
        .reverse() ?? [],
  );

  protected copy(): void {
    this.clipboard.copy(this.shortUrl());
    this.snackBar.open('Link copied', undefined, { duration: 2000 });
  }
}
