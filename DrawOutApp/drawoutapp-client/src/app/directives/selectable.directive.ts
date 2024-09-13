import { Directive, ElementRef, EventEmitter, HostListener, Input, OnChanges, Output, SimpleChange, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[appSelectable]',
  standalone: true
})
export class SelectableDirective implements OnChanges {

  @Input() selectableValue: any; // The value associated with this selectable item
  @Input() selectedValue: any; // The currently selected value
  @Output() selectedValueChange = new EventEmitter<any>(); // Emits when a new item is selected

  constructor(private el: ElementRef) { }

  @HostListener('click')
  onClick() {
    //this.el.nativeElement.classList.add('selected');
    this.selectedValueChange.emit(this.selectableValue);
    this.updateSelectedState();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['selectedValue']) this.updateSelectedState();
  }

  private updateSelectedState() {
    if (this.selectableValue === this.selectedValue) {
      this.el.nativeElement.classList.add('selected');
    } else {
      this.el.nativeElement.classList.remove('selected');
    }
  }

}
