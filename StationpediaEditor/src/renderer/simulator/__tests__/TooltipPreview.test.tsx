/**
 * Tests for TooltipPreview component
 */
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { TooltipPreview } from '../TooltipPreview';
import type { TooltipDefinition } from '@models/tooltipModel';

describe('TooltipPreview', () => {
  const mockTooltip: TooltipDefinition = {
    key: 'ItemBattery',
    description: 'Standard rechargeable power cell',
    category: 'Item',
  };

  it('should render tooltip content', () => {
    render(<TooltipPreview tooltip={mockTooltip} />);
    expect(screen.getByText('Standard rechargeable power cell')).toBeInTheDocument();
  });

  it('should display message when tooltip is null', () => {
    render(<TooltipPreview tooltip={null} />);
    expect(screen.getByText('Select a tooltip to preview')).toBeInTheDocument();
  });

  it('should display multiline descriptions', () => {
    const multilineTooltip: TooltipDefinition = {
      key: 'ItemBattery',
      description: 'Line 1\nLine 2\nLine 3',
    };

    render(<TooltipPreview tooltip={multilineTooltip} />);
    expect(screen.getByText(/Line 1/)).toBeInTheDocument();
  });

  it('should respect maxWidth prop', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} maxWidth={200} />);

    const tooltipContainer = container.querySelector('div[style*="max-width"]');
    expect(tooltipContainer).toHaveStyle('max-width: 200px');
  });

  it('should have tooltip styling classes', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} />);

    const tooltipDiv = container.querySelector('div[style*="max-width"]');
    expect(tooltipDiv).toHaveClass('rounded', 'border', 'bg-gray-950', 'shadow-lg');
  });

  it('should have text content in gray color', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} />);

    const textDiv = container.querySelector('div.text-gray-200');
    expect(textDiv).toBeInTheDocument();
    expect(textDiv).toHaveClass('text-gray-200');
  });

  it('should have pointer/arrow element', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} />);

    const arrow = container.querySelector('div[style*="width: 6px"]');
    expect(arrow).toBeInTheDocument();
  });

  it('should update content when tooltip changes', () => {
    const { rerender } = render(<TooltipPreview tooltip={mockTooltip} />);
    expect(screen.getByText('Standard rechargeable power cell')).toBeInTheDocument();

    const newTooltip: TooltipDefinition = {
      key: 'ItemCanister',
      description: 'Portable gas storage',
    };

    rerender(<TooltipPreview tooltip={newTooltip} />);
    expect(screen.getByText('Portable gas storage')).toBeInTheDocument();
    expect(screen.queryByText('Standard rechargeable power cell')).not.toBeInTheDocument();
  });

  it('should handle long descriptions', () => {
    const longTooltip: TooltipDefinition = {
      key: 'ItemBattery',
      description:
        'This is a very long description that might wrap to multiple lines in the tooltip preview. It should still display correctly and maintain proper text wrapping.',
    };

    render(<TooltipPreview tooltip={longTooltip} />);
    expect(
      screen.getByText(
        /This is a very long description that might wrap to multiple lines/
      )
    ).toBeInTheDocument();
  });

  it('should default maxWidth to 300', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} />);

    const tooltipContainer = container.querySelector('div[style*="max-width"]');
    expect(tooltipContainer).toHaveStyle('max-width: 300px');
  });

  it('should have proper text formatting classes', () => {
    const { container } = render(<TooltipPreview tooltip={mockTooltip} />);

    const textDiv = container.querySelector('div.text-gray-200');
    expect(textDiv).toHaveClass('whitespace-pre-wrap');
    expect(textDiv).toHaveClass('break-words');
  });

  it('should handle empty description', () => {
    const emptyTooltip: TooltipDefinition = {
      key: 'ItemEmpty',
      description: '',
    };

    const { container } = render(<TooltipPreview tooltip={emptyTooltip} />);
    const tooltipContainer = container.querySelector('div[style*="max-width"]');
    expect(tooltipContainer).toBeInTheDocument();
  });
});
