import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderListComponent } from './order-list.component';
import { OrderService } from '../../services/order.service';

describe('OrderListComponent', () => {
  let component: OrderListComponent;
  let fixture: ComponentFixture<OrderListComponent>;
  let orderService: OrderService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderListComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(OrderListComponent);
    component = fixture.componentInstance;
    orderService = TestBed.inject(OrderService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize mock data on init', () => {
    spyOn(orderService, 'initializeMockData');
    component.ngOnInit();
    expect(orderService.initializeMockData).toHaveBeenCalled();
  });

  it('should toggle between active and completed views', () => {
    expect(component.showCompleted()).toBe(false);
    component.toggleView();
    expect(component.showCompleted()).toBe(true);
    component.toggleView();
    expect(component.showCompleted()).toBe(false);
  });
});
