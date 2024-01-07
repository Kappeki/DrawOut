import { Component } from '@angular/core';
import { ViewChild, AfterViewInit } from '@angular/core';
import { FabricDirective } from 'ngx-fabric-wrapper';
import { fabric } from 'fabric';

@Component({
  selector: 'app-drawing',
  templateUrl: './drawing.component.html',
  styleUrl: './drawing.component.css'
})
export class DrawingComponent {

  // @ViewChild(FabricDirective) fabricDirective: FabricDirective;

  // public options: any = {
  //   isDrawingMode: true,
  //   // Other options...
  // };

  // ngAfterViewInit() {
  //   // Access canvas instance and set up
  //   const canvas = this.fabricDirective.getFabric();
}
