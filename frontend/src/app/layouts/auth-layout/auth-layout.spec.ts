import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Translation, TranslocoLoader, provideTransloco } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';

import { AuthLayout } from './auth-layout';

class TranslocoTestingLoader implements TranslocoLoader {
  getTranslation(): Observable<Translation> {
    return of({});
  }
}

describe('AuthLayout', () => {
  let component: AuthLayout;
  let fixture: ComponentFixture<AuthLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthLayout],
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
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
