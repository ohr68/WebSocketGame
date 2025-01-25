import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {DomSanitizer, SafeHtml} from '@angular/platform-browser';

@Component({
  selector: "app-raw-html",
  templateUrl: "./raw-html.component.html"
})

export class RawHtmlComponent implements OnChanges{
  @Input() htmlContent!: string;
  safeHtmlContent!: SafeHtml;

  constructor(private sanitizer: DomSanitizer) {}

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['htmlContent'] && this.htmlContent) {
      this.safeHtmlContent = this.sanitizer.bypassSecurityTrustHtml(this.htmlContent);
    }
  }
}
