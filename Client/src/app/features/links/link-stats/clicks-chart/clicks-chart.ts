import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { DailyClicks } from '../../link.models';

interface Bar {
  date: string;
  clicks: number;
  heightPercent: number;
  tooltipAlign: 'start' | 'center' | 'end';
}

/** Rounds up to the next 1, 2 or 5 x 10^n so the axis shows friendly numbers. */
function niceCeiling(value: number): number {
  if (value <= 1) {
    return 1;
  }

  const magnitude = 10 ** Math.floor(Math.log10(value));
  const step = [1, 2, 5, 10].find((s) => s * magnitude >= value)!;

  return step * magnitude;
}

@Component({
  selector: 'app-clicks-chart',
  imports: [DatePipe],
  templateUrl: './clicks-chart.html',
  styleUrl: './clicks-chart.scss',
})
export class ClicksChart {
  readonly daily = input.required<DailyClicks[]>();

  protected readonly max = computed(() =>
    niceCeiling(Math.max(0, ...this.daily().map((d) => d.clicks))),
  );

  protected readonly ticks = computed(() => {
    const max = this.max();
    return max % 2 === 0 ? [max, max / 2] : [max];
  });

  protected readonly bars = computed<Bar[]>(() => {
    const daily = this.daily();
    const max = this.max();

    return daily.map((day, index) => ({
      date: day.date,
      clicks: day.clicks,
      heightPercent: (day.clicks / max) * 100,
      tooltipAlign:
        index < daily.length / 4 ? 'start' : index >= (daily.length * 3) / 4 ? 'end' : 'center',
    }));
  });

  protected readonly firstDate = computed(() => this.daily().at(0)?.date);
  protected readonly lastDate = computed(() => this.daily().at(-1)?.date);
}
