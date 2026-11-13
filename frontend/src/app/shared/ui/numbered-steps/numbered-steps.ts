import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface NumberedStep {
  readonly title: string;
  readonly text: string;
}

/** Pink panel with white step cards numbered 01 / 02 / 03 (STEM Ambassadors path). */
@Component({
  selector: 'sd-numbered-steps',
  templateUrl: './numbered-steps.html',
  styleUrl: './numbered-steps.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NumberedSteps {
  readonly steps = input.required<readonly NumberedStep[]>();

  protected number(index: number): string {
    return (index + 1).toString().padStart(2, '0');
  }
}
