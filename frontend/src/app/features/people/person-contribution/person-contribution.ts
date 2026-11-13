import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** "Contribution to the project" box; one ✦ bullet per line of the CMS text. */
@Component({
  selector: 'sd-person-contribution',
  template: `
    <ul class="sd-sparkle-list" role="list">
      @for (line of lines(); track $index) {
        <li>{{ line }}</li>
      }
    </ul>
  `,
  styles: `
    :host {
      display: block;
      padding: var(--sd-space-24);
      border: 1px solid var(--sd-border);
      border-radius: var(--sd-radius-card);
      background: var(--sd-soft);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonContribution {
  readonly text = input.required<string>();

  protected readonly lines = computed(() =>
    this.text()
      .split(/\r?\n/)
      .map((line) => line.replace(/^\s*[-•✦*]\s*/, '').trim())
      .filter((line) => line.length > 0),
  );
}
