import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { TextService } from '../services/text.service';
import { TextsService } from '../services/texts.service';
import { UserService } from '../services/user.service';
import { TextWriteInfo } from './text-write-info';

@Component({
  selector: 'app-text-write-result',
  templateUrl: './text-write-result.component.html',
  styleUrls: ['../text-write/text-write.component.scss']
})
export class TextWriteResultComponent {
  public textWriteInfo = TextWriteInfo;
  text?: TextService;
  bestInfo?: TextWriteInfo;
  
  constructor(private router: Router, user: UserService, textsService: TextsService, titleService: Title) { 
    titleService.setTitle("Результаты печти");
    if(!this.textWriteInfo.lastTextInfo)
      router.navigateByUrl("");
    textsService.getTextById(this.textWriteInfo.lastTextInfo!.textId).then(text=>
    {
      if(!text)
        router.navigateByUrl("");
        titleService.setTitle(`Результаты печати текста ${text?.title}`);
      this.text = text!;
      if(user.isAuthorized)
      {
        user.updateTextWriteInfoAndGetBest(this.textWriteInfo.lastTextInfo).then(result=> {
          this.bestInfo = result;
          this.bestInfo.countOfAllChars = this.textWriteInfo.lastTextInfo.countOfAllChars;
        });
      }
    });
  }
  repeat() {
    this.router.navigate(['text-write'], 
    {
      queryParams:{
        "textId": TextWriteInfo.lastTextInfo.textId,
        "textSizeId": TextWriteInfo.lastTextInfo.textSize ?? 5,
        "actionTypeId": TextWriteInfo.lastTextInfo.textWriteType
      }
    });
  }
}
