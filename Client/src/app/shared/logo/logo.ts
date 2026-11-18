import { Component } from '@angular/core';

/** Brand mark: two interlocking chain links, plus the wordmark. */
@Component({
  selector: 'app-logo',
  template: `
    <svg viewBox="0 0 32 32" aria-hidden="true">
      <rect width="32" height="32" rx="7" class="tile" />
      <g transform="rotate(-45 16 16)" class="glyph">
        <rect x="5" y="12.5" width="13" height="7" rx="3.5" />
        <rect x="14" y="12.5" width="13" height="7" rx="3.5" />
      </g>
    </svg>
    <span>shortlink</span>
  `,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      font-weight: 600;
      font-size: 17px;
      letter-spacing: -0.02em;
      color: inherit;
    }

    svg {
      width: 28px;
      height: 28px;
    }

    .tile {
      fill: var(--app-accent);
    }

    .glyph rect {
      fill: none;
      stroke: #fff;
      stroke-width: 2.4;
    }
  `,
})
export class Logo {}
