import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaginatonComponent } from './paginaton.component';

describe('PaginatonComponent', () => {
  let component: PaginatonComponent;
  let fixture: ComponentFixture<PaginatonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaginatonComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PaginatonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
