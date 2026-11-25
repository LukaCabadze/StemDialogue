import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ViewportScroll } from '../../../core/browser/viewport-scroll.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { Avatar } from '../avatar/avatar';
import { Icon } from '../icon/icon';
import { DeckPerson } from './person-deck.model';

/** Resting pose for each depth in the stack (front card first). */
const POSES: readonly string[] = [
  'translate(0, 0) rotate(0deg) scale(1)',
  'translate(-3%, 3%) rotate(-5deg) scale(0.97)',
  'translate(3%, -4%) rotate(4deg) scale(0.95)',
  'translate(1%, 6%) rotate(-2deg) scale(0.93)',
];
const FLY_OUT = 'translate(-8%, -120%) rotate(-14deg) scale(0.96)';
const FLY_MS = 420;

/**
 * Stack of person cards on a blue panel. Clicking the front card throws it up and off the stack
 * and the next person comes forward; the dashes on the side jump straight to a person.
 */
@Component({
  selector: 'sd-person-deck',
  imports: [RouterLink, Avatar, Icon],
  templateUrl: './person-deck.html',
  styleUrl: './person-deck.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(keydown.arrowRight)': 'next()',
    '(keydown.arrowLeft)': 'previous()',
  },
})
export class PersonDeck {
  private readonly scroll = inject(ViewportScroll);
  protected readonly text = inject(LanguageService).text;

  readonly people = input.required<readonly DeckPerson[]>();
  readonly label = input.required<string>();

  private readonly position = signal(0);
  protected readonly flying = signal<string | null>(null);
  private timer: ReturnType<typeof setTimeout> | null = null;

  protected readonly count = computed(() => this.people().length);
  /** Index of the front card (kept in range when the list grows or shrinks). */
  protected readonly current = computed(() => {
    const count = this.count();
    return count === 0 ? 0 : ((this.position() % count) + count) % count;
  });
  protected readonly front = computed(() => this.people()[this.current()] ?? null);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clearTimer());
  }

  protected depth(index: number): number {
    const count = this.count();
    return (index - this.current() + count) % count;
  }

  protected transform(person: DeckPerson, index: number): string {
    if (this.flying() === person.key) {
      return FLY_OUT;
    }
    return POSES[Math.min(this.depth(index), POSES.length - 1)]!;
  }

  protected zIndex(person: DeckPerson, index: number): number {
    return this.flying() === person.key ? 200 : 100 - this.depth(index);
  }

  protected isHidden(person: DeckPerson, index: number): boolean {
    return this.flying() !== person.key && this.depth(index) >= POSES.length;
  }

  protected onCardClick(index: number, event: MouseEvent): void {
    if ((event.target as HTMLElement).closest('a')) {
      return;
    }
    if (this.depth(index) === 0) {
      this.next();
    } else {
      this.goTo(index);
    }
  }

  next(): void {
    const front = this.front();
    if (!front || this.count() < 2 || this.flying()) {
      return;
    }
    if (this.scroll.reducedMotion()) {
      this.position.update((value) => value + 1);
      return;
    }
    this.flying.set(front.key);
    this.timer = setTimeout(() => {
      this.position.update((value) => value + 1);
      this.flying.set(null);
      this.timer = null;
    }, FLY_MS);
  }

  previous(): void {
    if (this.count() < 2 || this.flying()) {
      return;
    }
    this.position.update((value) => value - 1);
  }

  protected goTo(index: number): void {
    if (this.flying()) {
      return;
    }
    this.position.set(index);
  }

  private clearTimer(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
