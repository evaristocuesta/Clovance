import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Translation, TranslocoLoader, provideTransloco } from '@jsverse/transloco';

import { ChangePassword } from './change-password';
import { AuthService } from '@core/services/auth.service';
import { ToastService } from '@shared/ui/toast/toast.service';

class TranslocoTestingLoader implements TranslocoLoader {
  getTranslation(): any {
    return of({});
  }
}

describe('ChangePassword', () => {
  let component: ChangePassword;
  let fixture: ComponentFixture<ChangePassword>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChangePassword],
      providers: [
        provideTransloco({
          config: {
            availableLangs: ['en', 'es'],
            defaultLang: 'en',
            reRenderOnLangChange: true,
            prodMode: true,
          },
          loader: TranslocoTestingLoader,
        }),
        {
          provide: AuthService,
          useValue: {
            changePassword: () => of(void 0),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangePassword);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('has a ToastService available', () => {
    expect(TestBed.inject(ToastService)).toBeTruthy();
  });
});
