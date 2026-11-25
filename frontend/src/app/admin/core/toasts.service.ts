import { Injectable, signal } from '@angular/core';

export interface Toast {
  readonly id: number;
  readonly kind: 'success' | 'error' | 'info';
  readonly text: string;
}

/** Short confirmations in the corner of the CMS ("შენახულია"). */
@Injectable()
export class Toasts {
  private next = 1;
  readonly items = signal<readonly Toast[]>([]);

  success(text: string): void {
    this.push('success', text);
  }

  error(text: string): void {
    this.push('error', text, 8000);
  }

  info(text: string): void {
    this.push('info', text);
  }

  dismiss(id: number): void {
    this.items.update((items) => items.filter((toast) => toast.id !== id));
  }

  private push(kind: Toast['kind'], text: string, ttl = 4000): void {
    const id = this.next++;
    this.items.update((items) => [...items, { id, kind, text }]);
    setTimeout(() => this.dismiss(id), ttl);
  }
}
