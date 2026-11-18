import { Clipboard } from '@angular/cdk/clipboard';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { filter, switchMap } from 'rxjs';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';
import { LinkFormDialog, LinkFormDialogData } from '../link-form-dialog/link-form-dialog';
import { Link } from '../link.models';
import { LinksService } from '../links.service';
import { shortLinkHost, shortUrl, splitUrl } from '../short-url';

@Component({
  selector: 'app-link-list',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatProgressBarModule,
    MatTableModule,
    MatTooltipModule,
  ],
  templateUrl: './link-list.html',
  styleUrl: './link-list.scss',
})
export class LinkList {
  private readonly linksService = inject(LinksService);
  private readonly dialog = inject(MatDialog);
  private readonly clipboard = inject(Clipboard);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  protected readonly columns = ['slug', 'destination', 'createdAt', 'actions'];
  protected readonly shortLinkHost = shortLinkHost;
  protected readonly splitUrl = splitUrl;

  protected readonly links = rxResource({
    stream: () => this.linksService.list(),
    defaultValue: [],
  });

  protected readonly query = signal('');

  protected readonly visibleLinks = computed(() => {
    const query = this.query().trim().toLowerCase();
    const links = this.links.value();

    if (!query) {
      return links;
    }

    return links.filter(
      (link) => link.slug.includes(query) || link.destinationUrl.toLowerCase().includes(query),
    );
  });

  protected open(link: Link): void {
    void this.router.navigate(['/links', link.id]);
  }

  protected copy(link: Link): void {
    this.clipboard.copy(shortUrl(link));
    this.notify(`Copied ${shortLinkHost}/${link.slug}`);
  }

  protected openCreate(): void {
    this.openForm({}, 'Link created');
  }

  protected openEdit(link: Link): void {
    this.openForm({ link }, 'Destination updated');
  }

  protected confirmDelete(link: Link): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: {
          title: `Delete /${link.slug}?`,
          message:
            'Anyone who opens this link will get a "not found" page. Its click history is deleted too.',
          confirmText: 'Delete link',
          destructive: true,
        },
      })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        switchMap(() => this.linksService.delete(link.id)),
      )
      .subscribe({
        next: () => {
          this.links.reload();
          this.notify('Link deleted');
        },
        error: () => this.notify('Could not delete the link'),
      });
  }

  private openForm(data: LinkFormDialogData, successMessage: string): void {
    this.dialog
      .open<LinkFormDialog, LinkFormDialogData, Link>(LinkFormDialog, { data, width: '480px' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.links.reload();
          this.notify(successMessage);
        }
      });
  }

  private notify(message: string): void {
    this.snackBar.open(message, undefined, { duration: 2500 });
  }
}
