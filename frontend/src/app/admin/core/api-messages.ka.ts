// Georgian wording for the API's validation and rule messages (the API answers in English).
// Unknown messages are shown as they are.

const exact: Readonly<Record<string, string>> = {
  'A button needs a link.': 'ღილაკს სჭირდება ბმული.',
  'A cancelled event cannot be completed. Restore it first.': 'გაუქმებულ ღონისძიებას ვერ მონიშნავთ ჩატარებულად. ჯერ აღადგინეთ.',
  'A completed event cannot be cancelled.': 'ჩატარებული ღონისძიება ვერ გაუქმდება.',
  'A completed event cannot be postponed.': 'ჩატარებული ღონისძიება ვერ გადაიტანება.',
  'A gallery can hold at most 100 images.': 'გალერეაში მაქსიმუმ 100 სურათია დასაშვები.',
  'A gallery can hold at most 60 images.': 'გალერეაში მაქსიმუმ 60 სურათია დასაშვები.',
  'A person can be added to an event only once.': 'ერთი ადამიანი ღონისძიებას მხოლოდ ერთხელ შეიძლება დაემატოს.',
  'A record with the same unique value already exists.': 'ასეთი ჩანაწერი (მაგ. იგივე slug) უკვე არსებობს.',
  'A venue is required for in-person and hybrid events.': 'ფიზიკური და ჰიბრიდული ღონისძიებისთვის მიუთითეთ ადგილი.',
  'Add a Georgian summary before publishing.': 'გამოქვეყნებამდე დაამატეთ ქართული ანოტაცია.',
  'Add a Georgian summary or description before publishing.': 'გამოქვეყნებამდე დაამატეთ ქართული მოკლე ან დეტალური აღწერა.',
  'Add the Georgian text before publishing.': 'გამოქვეყნებამდე დაამატეთ ქართული ტექსტი.',
  'An account with this email already exists.': 'ამ ელფოსტით ანგარიში უკვე არსებობს.',
  'An event can be completed only after it has started.': 'ღონისძიება ჩატარებულად შეიძლება მოინიშნოს მხოლოდ დაწყების შემდეგ.',
  'An event can last at most 7 days.': 'ღონისძიება შეიძლება გაგრძელდეს მაქსიმუმ 7 დღე.',
  'An organization can be listed only once.': 'ორგანიზაცია მხოლოდ ერთხელ შეიძლება იყოს სიაში.',
  'Archive the profile before deleting it.': 'წაშლამდე პროფილი დაარქივეთ.',
  'Archive the resource before deleting it.': 'წაშლამდე რესურსი დაარქივეთ.',
  'At least one active administrator must remain.': 'სისტემაში უნდა დარჩეს მინიმუმ ერთი აქტიური ადმინისტრატორი.',
  'At most 10 links.': 'მაქსიმუმ 10 ბმული.',
  'At most 20 tags.': 'მაქსიმუმ 20 თეგი.',
  'Choose a new password.': 'აირჩიეთ ახალი პაროლი.',
  'Confirm that consent and permissions exist before publishing photos from school activities.':
    'სასკოლო აქტივობის ფოტოების გამოქვეყნებამდე მონიშნეთ, რომ თანხმობა მიღებულია.',
  'Each role can be assigned once per project.': 'ერთი როლი ერთ პროექტში მხოლოდ ერთხელ შეიძლება მიენიჭოს.',
  'End date must not be before the start date.': 'დასრულების თარიღი დაწყებამდე ვერ იქნება.',
  'The end date must not be before the start date.': 'დასრულების თარიღი დაწყებამდე ვერ იქნება.',
  'Every gallery item must be an uploaded image.': 'გალერეაში მხოლოდ ატვირთული სურათები შეიძლება იყოს.',
  'Georgian text is required.': 'ქართული ტექსტი სავალდებულოა.',
  'Must be an absolute https:// URL.': 'უნდა იყოს სრული https:// მისამართი.',
  "Must be an https:// URL, a mailto: link or a site path starting with '/'.": 'უნდა იყოს https:// მისამართი, mailto: ბმული ან საიტის გზა, რომელიც „/“-ით იწყება.',
  'One or more disciplines do not exist.': 'არჩეული მიმართულება აღარ არსებობს — განაახლეთ გვერდი.',
  'One or more organizations do not exist.': 'არჩეული ორგანიზაცია აღარ არსებობს — განაახლეთ გვერდი.',
  'One or more people do not exist.': 'არჩეული ადამიანი აღარ არსებობს — განაახლეთ გვერდი.',
  'One or more projects do not exist.': 'არჩეული პროექტი არ არსებობს.',
  'One or more tags do not exist.': 'არჩეული თეგი აღარ არსებობს — განაახლეთ გვერდი.',
  'Only a cancelled event can be restored.': 'აღდგენა შესაძლებელია მხოლოდ გაუქმებული ღონისძიების.',
  'Provide either an uploaded file or an external link, not both.': 'მიუთითეთ ან ატვირთული ფაილი, ან გარე ბმული — არა ორივე.',
  'Published articles cannot be deleted. Archive the article instead.': 'გამოქვეყნებული სიახლე ვერ წაიშლება — დაარქივეთ.',
  'Published events cannot be deleted. Cancel or unpublish the event instead.': 'გამოქვეყნებული ღონისძიება ვერ წაიშლება — გააუქმეთ ან მოხსენით საიტიდან.',
  'Resources or news are linked to this event. Unlink them first.': 'ამ ღონისძიებას უკავშირდება რესურსები ან სიახლეები — ჯერ მოხსენით კავშირი.',
  'Send every milestone id exactly once.': 'ეტაპების სია მოძველდა — განაახლეთ გვერდი.',
  'The account is not active.': 'ანგარიში გათიშულია.',
  'The concurrency stamp is required when updating.': 'გადატვირთეთ გვერდი და სცადეთ თავიდან.',
  'The end must be after the start.': 'დასრულება უნდა იყოს დაწყების შემდეგ.',
  'The file was rejected.': 'ფაილი არ მიიღება (ტიპი ან შიგთავსი არ შეესაბამება).',
  'The new date must be in the future.': 'ახალი თარიღი მომავალში უნდა იყოს.',
  'The selected event does not exist in this project.': 'არჩეული ღონისძიება ამ პროექტში არ არსებობს.',
  'The selected file does not exist.': 'არჩეული ფაილი აღარ არსებობს.',
  'The selected file is not an image.': 'არჩეული ფაილი სურათი არ არის.',
  'The selected person does not exist.': 'არჩეული ადამიანი აღარ არსებობს.',
  'The session has expired. Sign in again.': 'სესია ამოიწურა. შედით თავიდან.',
  'This item was changed by someone else. Reload it and apply your edits again.':
    'ჩანაწერი სხვამ შეცვალა. გადატვირთეთ გვერდი და შეიტანეთ ცვლილებები თავიდან.',
  'This organization is a project partner. Remove it from the project first.': 'ორგანიზაცია პროექტის პარტნიორია — ჯერ მოხსენით პროექტის პარამეტრებში.',
  'This person is linked to events. Set an end date on their role or archive the profile instead.':
    'ადამიანი ღონისძიებებს უკავშირდება — მიუთითეთ როლის დასრულების თარიღი ან დაარქივეთ პროფილი.',
  'This person is the author of resources. Unlink them first.': 'ადამიანი რესურსების ავტორია — ჯერ მოხსენით კავშირი.',
  'This record is referenced by other content and cannot be changed this way.': 'ჩანაწერს სხვა კონტენტი უკავშირდება და ასე ვერ შეიცვლება.',
  'Videos and links must use an external URL; large media belongs on a video platform.':
    'ვიდეო და ბმული უნდა მიეთითოს გარე მისამართით (YouTube/Vimeo).',
  'You cannot remove your own administrator access or deactivate yourself.': 'საკუთარ ადმინისტრატორის უფლებას ან ანგარიშს ვერ გათიშავთ.',
  'Choose a file to upload.': 'აირჩიეთ ასატვირთი ფაილი.',
};

const patterns: readonly [RegExp, (m: RegExpMatchArray) => string][] = [
  [/^Must be at most (\d+) characters in each language\.$/, (m) => `მაქსიმუმ ${m[1]} სიმბოლო თითოეულ ენაზე.`],
  [/^Use at least (\d+) characters including letters and digits\.$/, (m) => `მინიმუმ ${m[1]} სიმბოლო, ასოებითა და ციფრებით.`],
  [/^Use lowercase latin letters, digits and single dashes/, () => 'Slug: მხოლოდ პატარა ლათინური ასოები, ციფრები და ტირე.'],
  [/^The file is too large\. The maximum is (\d+) MB\.$/, (m) => `ფაილი ძალიან დიდია. მაქსიმუმ ${m[1]} MB.`],
  [/^The file is still used \((.*)\)\. Remove it from that content first\.$/, (m) => `ფაილი ჯერ კიდევ გამოიყენება (${m[1]}). ჯერ იქიდან მოხსენით.`],
  [/^'(.+)' must not be empty\.$/, () => 'ველი სავალდებულოა.'],
  [/^'(.+)' is not a valid email address\.$/, () => 'ელფოსტა არასწორია.'],
];

export function toGeorgian(message: string): string {
  const known = exact[message];
  if (known) {
    return known;
  }
  for (const [pattern, render] of patterns) {
    const match = message.match(pattern);
    if (match) {
      return render(match);
    }
  }
  return message;
}
