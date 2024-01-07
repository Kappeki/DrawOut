import { Component, Input, Output, EventEmitter } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';

@Component({
  selector: 'app-paginaton',
  templateUrl: './paginaton.component.html',
  styleUrl: './paginaton.component.css'
})

export class PaginatonComponent {

  @Input() currentPage: number = 1;
  @Input() totalPages: number = 10;
  @Output() pageChange = new EventEmitter<number>();
  isSmallScreen: boolean =  window.innerWidth < 768;

  constructor(private breakpointObserver: BreakpointObserver) {
  }

  ngOnInit() {
    this.breakpointObserver
      .observe([Breakpoints.XSmall, Breakpoints.Small])
      .subscribe(result => {
        this.isSmallScreen = result.matches;
      }
    );
  }

  selectPage(page: number) {
    if (page !== this.currentPage && page > 0 && page <= this.totalPages) {
      this.currentPage = page;
      this.pageChange.emit(this.currentPage);
    }
  }

  getPagesVisibleOnSmallScreen(): number[] {
    // Logic to return an array of page numbers for small screens
    // For example, just the current page, previous and next
    return [this.currentPage - 1, this.currentPage, this.currentPage + 1].filter(page => page > 0 && page <= this.totalPages);
  }

  getPagesVisibleOnLargeScreen(): number[] {
    // Logic to return an array of page numbers for large screens
    // This can include the current page, pages around it, and ellipses logic
    let pages = [];
    // Example logic for generating pages (needs to be adjusted for your needs):
    for (let i = this.currentPage - 2; i <= this.currentPage + 2; i++) {
      if (i > 0 && i <= this.totalPages) {
        pages.push(i);
      }
    }
    return pages;
  }
}
