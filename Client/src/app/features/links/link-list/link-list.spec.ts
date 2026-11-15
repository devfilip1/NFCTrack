import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkList } from './link-list';

describe('LinkList', () => {
  let component: LinkList;
  let fixture: ComponentFixture<LinkList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LinkList],
    }).compileComponents();

    fixture = TestBed.createComponent(LinkList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
