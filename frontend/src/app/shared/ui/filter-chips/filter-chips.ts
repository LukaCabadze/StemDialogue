import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface ChipOption<T extends string = string> {
  readonly value: T;
  readonly label: string;
  readonly count?: number | null;
}

/**
 * Single-select chip group (category tabs, scope switch). The active chip is ink-filled as on the
 * team page tabs. Bind with `[(value)]`.
 */
@Component({
  selector: 'sd-filter-chips',
  templateUrl: './filter-chips.html',
  styleUrl: './filter-chips.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterChips<T extends string = string> {
  readonly options = input.required<readonly ChipOption<T>[]>();
  readonly value = model<T | null>(null);
  readonly label = input.required<string>();

  protected select(option: ChipOption<T>): void {
    this.value.set(option.value);
  }
}
