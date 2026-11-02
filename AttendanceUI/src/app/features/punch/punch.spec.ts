import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Punch } from './punch';

describe('Punch', () => {
  let component: Punch;
  let fixture: ComponentFixture<Punch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Punch],
    }).compileComponents();

    fixture = TestBed.createComponent(Punch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
