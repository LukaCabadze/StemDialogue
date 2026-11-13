import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

import { Language } from '../models';
import { UI_TEXT_EN } from './ui-text.en';
import { UI_TEXT_KA } from './ui-text.ka';
import { UiText } from './ui-text.model';

const STORAGE_KEY = 'sd.language';
const SUPPORTED: readonly Language[] = ['ka', 'en'];

/** Current UI/content language. Georgian is the primary language of the site. */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly document = inject(DOCUMENT);
  private readonly current = signal<Language>(this.readStored());

  readonly language = this.current.asReadonly();
  readonly isGeorgian = computed(() => this.current() === 'ka');
  /** Static interface copy for the current language. */
  readonly text = computed<UiText>(() => (this.current() === 'en' ? UI_TEXT_EN : UI_TEXT_KA));

  constructor() {
    effect(() => {
      const language = this.current();
      this.document.documentElement.lang = language;
      this.writeStored(language);
    });
  }

  use(language: Language): void {
    if (SUPPORTED.includes(language)) {
      this.current.set(language);
    }
  }

  toggle(): void {
    this.use(this.current() === 'ka' ? 'en' : 'ka');
  }

  private readStored(): Language {
    try {
      const stored = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
      return stored === 'en' ? 'en' : 'ka';
    } catch {
      return 'ka';
    }
  }

  private writeStored(language: Language): void {
    try {
      this.document.defaultView?.localStorage.setItem(STORAGE_KEY, language);
    } catch {
      // Storage can be unavailable (private mode, embedded views); the language still works in memory.
    }
  }
}
