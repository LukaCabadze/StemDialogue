import { ChangeDetectionStrategy, Component, ElementRef, inject, input, output, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { Icon } from '../../shared/ui/icon/icon';
import { NavEntry } from './nav-items';

/** Primary menu. Desktop: horizontal with a "People" dropdown; mobile: stacked inside the drawer. */
@Component({
  selector: 'sd-main-nav',
  imports: [RouterLink, RouterLinkActive, Icon],
  templateUrl: './main-nav.html',
  styleUrl: './main-nav.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-stacked]': 'stacked()',
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'closeGroup()',
  },
})
export class MainNav {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly entries = input.required<readonly NavEntry[]>();
  readonly label = input.required<string>();
  readonly stacked = input(false);
  /** Emitted after any link is followed (closes the mobile drawer). */
  readonly navigate = output<void>();

  protected readonly openGroup = signal<string | null>(null);
  protected toggleGroup(key: string): void {
    this.openGroup.update((current) => (current === key ? null : key));
  }

  protected closeGroup(): void {
    this.openGroup.set(null);
  }

  protected followLink(): void {
    this.closeGroup();
    this.navigate.emit();
  }

  protected onDocumentClick(event: MouseEvent): void {
    if (this.openGroup() && !this.host.nativeElement.contains(event.target as Node)) {
      this.closeGroup();
    }
  }
}
