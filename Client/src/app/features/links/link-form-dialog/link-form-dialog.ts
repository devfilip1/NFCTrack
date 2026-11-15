import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Link } from '../link.models';
import { LinksService } from '../links.service';

export interface LinkFormDialogData {
  link?: Link;
}

const SLUG_PATTERN = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const URL_PATTERN = /^https?:\/\/.+/;

@Component({
  selector: 'app-link-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './link-form-dialog.html',
  styleUrl: './link-form-dialog.scss',
})
export class LinkFormDialog {
  private readonly linksService = inject(LinksService);
  private readonly dialogRef = inject<MatDialogRef<LinkFormDialog, Link>>(MatDialogRef);

  protected readonly data = inject<LinkFormDialogData>(MAT_DIALOG_DATA);
  protected readonly isEdit = this.data.link !== undefined;
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    destinationUrl: [
      this.data.link?.destinationUrl ?? '',
      [Validators.required, Validators.pattern(URL_PATTERN)],
    ],
    slug: [
      { value: this.data.link?.slug ?? '', disabled: this.isEdit },
      [Validators.minLength(3), Validators.maxLength(32), Validators.pattern(SLUG_PATTERN)],
    ],
  });

  constructor() {
    // A server error refers to the values that were sent; hide it as soon as the user edits.
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.error.set(null));
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const { destinationUrl, slug } = this.form.getRawValue();

    const request$ = this.isEdit
      ? this.linksService.updateDestination(this.data.link!.id, { destinationUrl })
      : this.linksService.create({ destinationUrl, slug: slug || undefined });

    request$.subscribe({
      next: (link) => this.dialogRef.close(link),
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(this.messageFor(err));
      },
    });
  }

  private messageFor(err: HttpErrorResponse): string {
    if (err.status === 409) {
      return 'This slug is already in use.';
    }

    if (err.status === 400) {
      return err.error?.detail ?? 'Invalid data.';
    }

    return 'Something went wrong. Please try again.';
  }
}
