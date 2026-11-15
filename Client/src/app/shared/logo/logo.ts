import { Component } from '@angular/core';

/** Brand mark: a dot (the card) with two NFC waves, plus the wordmark. */
@Component({
  selector: 'app-logo',
  template: `
    <svg viewBox="0 0 32 32" aria-hidden="true">
      <rect width="32" height="32" rx="7" class="tile" />
      <circle cx="10" cy="16" r="2.5" class="glyph-fill" />
      <path d="M15 10.5a7.5 7.5 0 0 1 0 11M19.5 7a12.5 12.5 0 0 1 0 18" class="glyph-stroke" />
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

    .glyph-fill {
      fill: #fff;
    }

    .glyph-stroke {
      fill: none;
      stroke: #fff;
      stroke-width: 2.4;
      stroke-linecap: round;
    }
  `,
})
export class Logo {}
