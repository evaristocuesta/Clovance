import { TestBed } from '@angular/core/testing';

import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('starts with no toasts', () => {
    expect(service.toasts()).toEqual([]);
  });

  it('adds a success toast', () => {
    service.success('Saved');

    expect(service.toasts()).toEqual([{ id: 0, variant: 'success', message: 'Saved' }]);
  });

  it('adds an error toast', () => {
    service.error('Something went wrong');

    expect(service.toasts()).toEqual([
      { id: 0, variant: 'error', message: 'Something went wrong' },
    ]);
  });

  it('adds an info toast', () => {
    service.info('Heads up');

    expect(service.toasts()).toEqual([{ id: 0, variant: 'info', message: 'Heads up' }]);
  });

  it('stacks multiple toasts with increasing ids', () => {
    service.success('First');
    service.error('Second');

    expect(service.toasts().map((toast) => toast.id)).toEqual([0, 1]);
  });

  it('removes a toast on dismiss', () => {
    service.success('Saved');

    service.dismiss(0);

    expect(service.toasts()).toEqual([]);
  });

  it('auto-dismisses a success toast after its default duration', () => {
    service.success('Saved');

    vi.advanceTimersByTime(4999);
    expect(service.toasts().length).toBe(1);

    vi.advanceTimersByTime(1);
    expect(service.toasts().length).toBe(0);
  });

  it('auto-dismisses an error toast after a longer default duration', () => {
    service.error('Oops');

    vi.advanceTimersByTime(5000);
    expect(service.toasts().length).toBe(1);

    vi.advanceTimersByTime(3000);
    expect(service.toasts().length).toBe(0);
  });

  it('never auto-dismisses a toast shown with duration 0', () => {
    service.success('Stays forever', 0);

    vi.advanceTimersByTime(60_000);

    expect(service.toasts().length).toBe(1);
  });

  it('pausing stops the auto-dismiss countdown', () => {
    service.success('Saved');

    vi.advanceTimersByTime(4000);
    service.pause(0);
    vi.advanceTimersByTime(10_000);

    expect(service.toasts().length).toBe(1);
  });

  it('resuming continues the countdown with the remaining time', () => {
    service.success('Saved');

    vi.advanceTimersByTime(4000);
    service.pause(0);
    vi.advanceTimersByTime(10_000);
    service.resume(0);

    vi.advanceTimersByTime(999);
    expect(service.toasts().length).toBe(1);

    vi.advanceTimersByTime(1);
    expect(service.toasts().length).toBe(0);
  });
});
