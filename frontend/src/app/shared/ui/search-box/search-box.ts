import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output } from '@angular/core';

import { Icon } from '../icon/icon';

/** Search input that emits the trimmed query after the visitor stops typing (300 ms). */
@Component({
  selector: 'sd-search-box',
  imports: [Icon],
  templateUrl: './search-box.html',
  styleUrl: './search-box.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBox {
  readonly placeholder = input.required<string>();
  readonly value = input('');
  readonly search = output<string>();

  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clearTimer());
  }

  protected onInput(raw: string): void {
    this.clearTimer();
    this.timer = setTimeout(() => this.search.emit(raw.trim()), 300);
  }

  protected onSubmit(raw: string, event: Event): void {
    event.preventDefault();
    this.clearTimer();
    this.search.emit(raw.trim());
  }

  private clearTimer(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
