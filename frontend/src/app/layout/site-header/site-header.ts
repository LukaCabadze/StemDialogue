import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';

import { LanguageService } from '../../core/i18n/language.service';
import { ProjectContext } from '../../core/project/project-context.service';
import { SiteStore } from '../../core/site/site.store';
import { Icon } from '../../shared/ui/icon/icon';
import { LanguageSwitch } from '../language-switch/language-switch';
import { MainNav } from '../main-nav/main-nav';
import { buildNavigation } from '../main-nav/nav-items';

/** Sticky 80px header: logo lock-up, main menu, language switch and the "join" call to action. */
@Component({
  selector: 'sd-site-header',
  imports: [RouterLink, Icon, LanguageSwitch, MainNav],
  templateUrl: './site-header.html',
  styleUrl: './site-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-menu-open]': 'menuOpen()',
    '(document:keydown.escape)': 'closeMenu()',
  },
})
export class SiteHeader {
  private readonly document = inject(DOCUMENT);
  private readonly site = inject(SiteStore);
  protected readonly project = inject(ProjectContext);
  protected readonly text = inject(LanguageService).text;

  protected readonly menuOpen = signal(false);
  protected readonly navigation = computed(() =>
    buildNavigation(this.project.rootPath(), this.text(), this.site.navigation()),
  );
  protected readonly joinPath = computed(() => this.project.path('stem-saturdays'));

  private readonly navigationEnd = toSignal(
    inject(Router).events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  constructor() {
    // Close the drawer after any navigation and lock page scroll while it is open.
    effect(() => {
      this.navigationEnd();
      this.menuOpen.set(false);
    });
    effect(() => {
      this.document.body.style.overflow = this.menuOpen() ? 'hidden' : '';
    });
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }
}
