import { ChangeDetectionStrategy, Component, ElementRef, effect, model, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

type Command = { readonly label: string; readonly title: string; readonly run: () => void };

/**
 * Minimal rich-text editor that produces only markup the API sanitizer keeps
 * (paragraphs, h2/h3, lists, quotes, links, bold/italic). "HTML" switches to source editing.
 */
@Component({
  selector: 'adm-rich-editor',
  imports: [FormsModule],
  template: `
    <div class="adm-rte">
      <div class="adm-rte__bar" role="toolbar" aria-label="ტექსტის ფორმატირება">
        @for (command of commands; track command.label) {
          <button type="button" class="adm-rte__btn" [title]="command.title" [disabled]="source()" (mousedown)="$event.preventDefault()" (click)="command.run()">
            {{ command.label }}
          </button>
        }
        <span class="adm-rte__spacer"></span>
        <button type="button" class="adm-rte__btn" [class.is-on]="source()" title="HTML კოდის რედაქტირება" (click)="toggleSource()">HTML</button>
      </div>
      @if (source()) {
        <textarea class="adm-rte__source" rows="12" [ngModel]="html() ?? ''" (ngModelChange)="html.set($event)"></textarea>
      } @else {
        <div
          #area
          class="adm-rte__area adm-prose"
          contenteditable="true"
          role="textbox"
          aria-multiline="true"
          (focus)="onFocus()"
          (input)="onInput()"
          (blur)="onInput()"
        ></div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RichEditor {
  readonly html = model<string | null>(null);
  protected readonly source = signal(false);
  private readonly area = viewChild<ElementRef<HTMLDivElement>>('area');

  protected readonly commands: readonly Command[] = [
    { label: 'B', title: 'მუქი', run: () => this.exec('bold') },
    { label: 'I', title: 'დახრილი', run: () => this.exec('italic') },
    { label: 'H2', title: 'სათაური', run: () => this.exec('formatBlock', 'h2') },
    { label: 'H3', title: 'ქვესათაური', run: () => this.exec('formatBlock', 'h3') },
    { label: '¶', title: 'აბზაცი', run: () => this.exec('formatBlock', 'p') },
    { label: '• სია', title: 'ბურთულებიანი სია', run: () => this.exec('insertUnorderedList') },
    { label: '1. სია', title: 'დანომრილი სია', run: () => this.exec('insertOrderedList') },
    { label: '❝', title: 'ციტატა', run: () => this.exec('formatBlock', 'blockquote') },
    { label: 'ბმული', title: 'ბმულის დამატება', run: () => this.link() },
    { label: '⌫ ბმული', title: 'ბმულის მოხსნა', run: () => this.exec('unlink') },
    { label: 'Tx', title: 'ფორმატის გასუფთავება', run: () => this.exec('removeFormat') },
  ];

  constructor() {
    // Push external value changes (load, language switch) into the editable area without moving the caret.
    effect(() => {
      const value = this.html() ?? '';
      const area = this.area()?.nativeElement;
      if (area && area.innerHTML !== value) {
        area.innerHTML = value;
      }
    });
  }

  protected onFocus(): void {
    document.execCommand('defaultParagraphSeparator', false, 'p');
  }

  protected onInput(): void {
    const area = this.area()?.nativeElement;
    if (!area) {
      return;
    }
    const value = area.innerHTML === '<br>' ? '' : area.innerHTML;
    if (value !== (this.html() ?? '')) {
      this.html.set(value);
    }
  }

  protected toggleSource(): void {
    this.onInput();
    this.source.update((on) => !on);
  }

  private exec(command: string, value?: string): void {
    this.area()?.nativeElement.focus();
    document.execCommand(command, false, value);
    this.onInput();
  }

  private link(): void {
    const url = prompt('ბმულის მისამართი (https://…)', 'https://');
    if (url && /^(https?:\/\/|mailto:)/i.test(url)) {
      this.exec('createLink', url);
    }
  }
}
