import { Component, input } from '@angular/core';
import { Logo } from '../../../shared/logo/logo';

/**
 * Two-column frame shared by the sign-in and registration pages.
 * The page's form is projected into the right column through <ng-content>.
 */
@Component({
  selector: 'app-auth-layout',
  imports: [Logo],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss',
})
export class AuthLayout {
  readonly heading = input.required<string>();
  readonly subheading = input<string>();
}
