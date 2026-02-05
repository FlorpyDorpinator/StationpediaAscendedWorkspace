/**
 * Comprehensive validation service for device descriptions
 * Validates TMP syntax, links, assets, and content structure
 */

import type {
  ValidationContext,
  ValidationError,
  ValidationRule,
  ValidationResult,
} from '@models/validationModel';
import { VALIDATION_RULE_IDS } from '@models/validationModel';

/**
 * Validator service with comprehensive validation rules
 */
export class ValidatorService {
  private rules: Map<string, ValidationRule> = new Map();

  constructor() {
    this.initializeRules();
  }

  /**
   * Initialize all validation rules
   */
  private initializeRules(): void {
    this.rules.set(VALIDATION_RULE_IDS.MISSING_DEVICE_KEY, {
      id: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
      name: 'Missing Device Key',
      description: 'Device key is required',
      severity: 'error',
      validate: (context) => this.validateMissingDeviceKey(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.UNCLOSED_TAG, {
      id: VALIDATION_RULE_IDS.UNCLOSED_TAG,
      name: 'Unclosed TMP Tag',
      description: 'TMP tag is not properly closed',
      severity: 'error',
      validate: (context) => this.validateUnclosedTags(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.INVALID_TMP_TAG, {
      id: VALIDATION_RULE_IDS.INVALID_TMP_TAG,
      name: 'Invalid TMP Tag',
      description: 'Invalid or unsupported TMP tag',
      severity: 'warning',
      validate: (context) => this.validateInvalidTmpTags(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.INVALID_COLOR, {
      id: VALIDATION_RULE_IDS.INVALID_COLOR,
      name: 'Invalid Color Format',
      description: 'Invalid hex color format in TMP',
      severity: 'warning',
      validate: (context) => this.validateColorFormat(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.BROKEN_LINK, {
      id: VALIDATION_RULE_IDS.BROKEN_LINK,
      name: 'Broken Link',
      description: 'Reference to non-existent device',
      severity: 'warning',
      validate: (context) => this.validateBrokenLinks(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.SELF_LINK, {
      id: VALIDATION_RULE_IDS.SELF_LINK,
      name: 'Self-Reference Link',
      description: 'Device linking to itself',
      severity: 'info',
      validate: (context) => this.validateSelfLinks(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.MISSING_ASSET, {
      id: VALIDATION_RULE_IDS.MISSING_ASSET,
      name: 'Missing Asset',
      description: 'Referenced image file does not exist',
      severity: 'warning',
      validate: (context) => this.validateMissingAssets(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.EMPTY_TITLE, {
      id: VALIDATION_RULE_IDS.EMPTY_TITLE,
      name: 'Empty Title',
      description: 'Operational detail section has no title',
      severity: 'warning',
      validate: (context) => this.validateEmptyTitles(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.EMPTY_CONTENT, {
      id: VALIDATION_RULE_IDS.EMPTY_CONTENT,
      name: 'Empty Content',
      description: 'Section has no content',
      severity: 'info',
      validate: (context) => this.validateEmptyContent(context),
    });

    this.rules.set(VALIDATION_RULE_IDS.DUPLICATE_TOC_ID, {
      id: VALIDATION_RULE_IDS.DUPLICATE_TOC_ID,
      name: 'Duplicate TOC ID',
      description: 'Multiple sections have the same table of contents ID',
      severity: 'error',
      validate: (context) => this.validateDuplicateTocIds(context),
    });
  }

  /**
   * Validate device with all rules
   */
  validate(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];

    for (const rule of this.rules.values()) {
      try {
        const ruleErrors = rule.validate(context);
        errors.push(...ruleErrors);
      } catch (error) {
        console.error(`Error running validation rule ${rule.id}:`, error);
      }
    }

    return errors;
  }

  /**
   * Create a validation result from errors
   */
  createResult(deviceKey: string, errors: ValidationError[]): ValidationResult {
    return {
      deviceKey,
      errors,
      hasErrors: errors.some(e => e.severity === 'error'),
      hasWarnings: errors.some(e => e.severity === 'warning'),
      hasInfo: errors.some(e => e.severity === 'info'),
    };
  }

  // ====== VALIDATION RULE IMPLEMENTATIONS ======

  private validateMissingDeviceKey(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    
    if (!context.device.deviceKey || context.device.deviceKey.trim() === '') {
      errors.push({
        ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
        severity: 'error',
        message: 'Device key is required',
        location: {
          field: 'deviceKey',
        },
      });
    }

    return errors;
  }

  private validateUnclosedTags(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const textContent = this.collectAllTextContent(context.device);

    for (const [text, location] of textContent) {
      if (!text) continue;

      // Track open/close tags
      const tagStack: { tag: string; pos: number }[] = [];
      const supportedTags = new Set(['b', 'i', 'u', 's', 'sup', 'sub', 'color', 'size', 'alpha']);

      // Match all tags: <tag> and </tag>
      const tagRegex = /<\/?([a-z]+)[^>]*>/gi;
      let match;

      while ((match = tagRegex.exec(text)) !== null) {
        const tag = match[1].toLowerCase();
        const isClosing = match[0].startsWith('</');

        if (!supportedTags.has(tag)) {
          // Invalid tag will be caught by validateInvalidTmpTags
          continue;
        }

        if (isClosing) {
          // Check if we have a matching open tag
          const openIndex = tagStack.findIndex(t => t.tag === tag);
          if (openIndex >= 0) {
            tagStack.splice(openIndex, 1);
          }
        } else {
          // Opening tag
          tagStack.push({ tag, pos: match.index });
        }
      }

      // Any remaining tags in stack are unclosed
      for (const unclosed of tagStack) {
        errors.push({
          ruleId: VALIDATION_RULE_IDS.UNCLOSED_TAG,
          severity: 'error',
          message: `Unclosed tag: <${unclosed.tag}>`,
          location,
        });
      }
    }

    return errors;
  }

  private validateInvalidTmpTags(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const textContent = this.collectAllTextContent(context.device);
    const supportedTags = new Set(['b', 'i', 'u', 's', 'sup', 'sub', 'color', 'size', 'alpha']);

    for (const [text, location] of textContent) {
      if (!text) continue;

      const tagRegex = /<\/?([a-z]+)[^>]*>/gi;
      let match;

      while ((match = tagRegex.exec(text)) !== null) {
        const tag = match[1].toLowerCase();
        if (!supportedTags.has(tag)) {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.INVALID_TMP_TAG,
            severity: 'warning',
            message: `Unsupported TMP tag: <${tag}>`,
            location,
          });
        }
      }
    }

    return errors;
  }

  private validateColorFormat(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const textContent = this.collectAllTextContent(context.device);

    for (const [text, location] of textContent) {
      if (!text) continue;

      // Match <color=#XXXXXX> format
      const colorRegex = /<color=([^>]+)>/gi;
      let match;

      while ((match = colorRegex.exec(text)) !== null) {
        const colorValue = match[1];

        // Valid hex colors: #RRGGBB or #RRGGBBAA
        if (!/^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$/.test(colorValue)) {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.INVALID_COLOR,
            severity: 'warning',
            message: `Invalid hex color format: ${colorValue}. Expected #RRGGBB or #RRGGBBAA`,
            location,
          });
        }
      }
    }

    return errors;
  }

  private validateBrokenLinks(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const textContent = this.collectAllTextContent(context.device);

    for (const [text, location] of textContent) {
      if (!text) continue;

      // Match {THING:Key} format
      const thingLinkRegex = /\{THING:([^}]+)\}/g;
      let match;

      while ((match = thingLinkRegex.exec(text)) !== null) {
        const linkedKey = match[1].trim();

        if (context.knownDeviceKeys.size > 0 && !context.knownDeviceKeys.has(linkedKey)) {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.BROKEN_LINK,
            severity: 'warning',
            message: `Broken link: Device key "${linkedKey}" not found`,
            location,
          });
        }
      }
    }

    return errors;
  }

  private validateSelfLinks(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const deviceKey = context.device.deviceKey;

    if (!deviceKey) return errors;

    const textContent = this.collectAllTextContent(context.device);

    for (const [text, location] of textContent) {
      if (!text) continue;

      const thingLinkRegex = /\{THING:([^}]+)\}/g;
      let match;

      while ((match = thingLinkRegex.exec(text)) !== null) {
        const linkedKey = match[1].trim();

        if (linkedKey === deviceKey) {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.SELF_LINK,
            severity: 'info',
            message: `Device links to itself: {THING:${deviceKey}}`,
            location,
          });
        }
      }
    }

    return errors;
  }

  private validateMissingAssets(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];

    const checkAsset = (imageFile: string | undefined, location: any) => {
      if (imageFile && context.assetRegistry.size > 0 && !context.assetRegistry.has(imageFile)) {
        errors.push({
          ruleId: VALIDATION_RULE_IDS.MISSING_ASSET,
          severity: 'warning',
          message: `Image file not found: "${imageFile}"`,
          location,
        });
      }
    };

    // Check page description images if applicable
    if (context.device.imageFile) {
      checkAsset(context.device.imageFile, { deviceKey: context.device.deviceKey, field: 'imageFile' });
    }

    // Check operational details images recursively
    const checkDetails = (details: any[], path: number[] = []) => {
      if (!details) return;

      details.forEach((detail, index) => {
        const currentPath = [...path, index];
        checkAsset(detail.imageFile, {
          deviceKey: context.device.deviceKey,
          field: 'operationalDetails.imageFile',
          sectionIndex: index,
          childPath: currentPath,
        });

        if (detail.children) {
          checkDetails(detail.children, currentPath);
        }
      });
    };

    if (context.device.operationalDetails) {
      checkDetails(context.device.operationalDetails);
    }

    return errors;
  }

  private validateEmptyTitles(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];

    const checkDetails = (details: any[], path: number[] = []) => {
      if (!details) return;

      details.forEach((detail, index) => {
        const currentPath = [...path, index];

        if (!detail.title || detail.title.trim() === '') {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.EMPTY_TITLE,
            severity: 'warning',
            message: `Operational detail at position [${currentPath.join('.')}] has no title`,
            location: {
              deviceKey: context.device.deviceKey,
              field: 'operationalDetails.title',
              sectionIndex: index,
              childPath: currentPath,
            },
          });
        }

        if (detail.children) {
          checkDetails(detail.children, currentPath);
        }
      });
    };

    if (context.device.operationalDetails) {
      checkDetails(context.device.operationalDetails);
    }

    return errors;
  }

  private validateEmptyContent(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];

    const checkDetails = (details: any[], path: number[] = []) => {
      if (!details) return;

      details.forEach((detail, index) => {
        const currentPath = [...path, index];
        const hasContent =
          detail.description ||
          (detail.items && detail.items.length > 0) ||
          (detail.steps && detail.steps.length > 0) ||
          (detail.children && detail.children.length > 0);

        if (!hasContent) {
          errors.push({
            ruleId: VALIDATION_RULE_IDS.EMPTY_CONTENT,
            severity: 'info',
            message: `"${detail.title || 'Untitled'}" has no content (description, items, steps, or children)`,
            location: {
              deviceKey: context.device.deviceKey,
              field: 'operationalDetails',
              sectionIndex: index,
              childPath: currentPath,
            },
          });
        }

        if (detail.children) {
          checkDetails(detail.children, currentPath);
        }
      });
    };

    if (context.device.operationalDetails) {
      checkDetails(context.device.operationalDetails);
    }

    return errors;
  }

  private validateDuplicateTocIds(context: ValidationContext): ValidationError[] {
    const errors: ValidationError[] = [];
    const tocIds = new Map<string, { path: number[]; title: string }>();

    const checkDetails = (details: any[], path: number[] = []) => {
      if (!details) return;

      details.forEach((detail, index) => {
        const currentPath = [...path, index];

        if (detail.tocId) {
          if (tocIds.has(detail.tocId)) {
            const first = tocIds.get(detail.tocId)!;
            errors.push({
              ruleId: VALIDATION_RULE_IDS.DUPLICATE_TOC_ID,
              severity: 'error',
              message: `Duplicate tocId "${detail.tocId}": already used in "${first.title}" at [${first.path.join('.')}]`,
              location: {
                deviceKey: context.device.deviceKey,
                field: 'operationalDetails.tocId',
                sectionIndex: index,
                childPath: currentPath,
              },
            });
          } else {
            tocIds.set(detail.tocId, { path: currentPath, title: detail.title });
          }
        }

        if (detail.children) {
          checkDetails(detail.children, currentPath);
        }
      });
    };

    if (context.device.operationalDetails) {
      checkDetails(context.device.operationalDetails);
    }

    return errors;
  }

  // ====== HELPER METHODS ======

  /**
   * Collect all text content from device for validation
   * Returns array of [text, location] tuples
   */
  private collectAllTextContent(device: any): Array<[string, any]> {
    const content: Array<[string, any]> = [];

    // Page description
    if (device.pageDescription) {
      content.push([device.pageDescription, { deviceKey: device.deviceKey, field: 'pageDescription' }]);
    }

    // Operational details recursively
    const collectDetails = (details: any[], path: number[] = []) => {
      if (!details) return;

      details.forEach((detail, index) => {
        const currentPath = [...path, index];

        if (detail.description) {
          content.push([
            detail.description,
            {
              deviceKey: device.deviceKey,
              field: 'operationalDetails.description',
              sectionIndex: index,
              childPath: currentPath,
            },
          ]);
        }

        if (detail.children) {
          collectDetails(detail.children, currentPath);
        }
      });
    };

    if (device.operationalDetails) {
      collectDetails(device.operationalDetails);
    }

    return content;
  }
}

export const validatorService = new ValidatorService();
